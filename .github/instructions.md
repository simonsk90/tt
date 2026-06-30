# TT.BackendTransformation — AI Instructions

> **This file is the single entry point for all AI coding assistants working in this repository.**
> Read this file before making ANY code change. Then read the relevant topic file from `.github/topics/`.

---

## Architecture Overview

This solution follows **Clean Architecture** with **Domain-Driven Design (DDD)** and is optimised for **AI-assisted engineering (AIDD)**. The four layers have strict dependency rules:

```
┌──────────────────────────────────────────────────────────┐
│  TT.Api          (Presentation / Composition Root)        │
│  ↓ depends on Application + Infrastructure               │
├──────────────────────────────────────────────────────────┤
│  TT.Infrastructure   (EF Core · Hangfire · SSE)          │
│  ↓ depends on Application + Domain                       │
├──────────────────────────────────────────────────────────┤
│  TT.Application      (MediatR Commands · Queries)        │
│  ↓ depends on Domain only                                │
├──────────────────────────────────────────────────────────┤
│  TT.Domain           (Aggregates · Events · Value Obj.)  │
│  ↓ NO external dependencies (pure C#)                    │
└──────────────────────────────────────────────────────────┘
```

---

## Hard Rules for AI Agents

### 🚫 TT.Domain — Forbidden dependencies
- MUST NOT reference MediatR, EF Core, Hangfire, or any NuGet package.
- MUST NOT reference Application or Infrastructure projects.
- `AggregateRoot` collects `DomainEvent` records internally; it does NOT dispatch them.
- Domain events are dispatched by `AppDbContext.SaveChangesAsync` (Infrastructure).

### 🚫 TT.Application — Forbidden dependencies
- MUST NOT reference EF Core (`DbContext`, `DbSet`, LINQ to SQL, migrations).
- MUST NOT reference Hangfire implementation types (only `IBackgroundJobClient` via interface).
- Defines repository interfaces (`IRobotRepository`), not implementations.
- May reference MediatR for `IRequest`, `INotification`, `INotificationHandler`.

### 🚫 TT.Infrastructure — Forbidden dependencies
- MUST NOT be referenced by TT.Domain or TT.Application (dependency flows inward).
- Is the ONLY layer allowed to use EF Core, Hangfire, and SSE infrastructure.

### ✅ TT.Api — Composition Root
- The only project that wires all layers together via `Program.cs`.
- Uses `AddInfrastructure()` extension and `AddMediatR()` to register services.
- Must not contain business logic.

---

## Before Modifying Domain Logic

**AI MUST read the relevant topic file:**

| Domain Area         | Topic File                                   |
|---------------------|----------------------------------------------|
| RouteComplete flow  | `.github/topics/RouteCompleteTopic.md`       |
| Robot aggregate     | `.github/topics/RouteCompleteTopic.md`       |

---

## Project Conventions

### Naming
- Commands: `{Verb}{Noun}Command` (e.g. `StartFieldMarkingCommand`)
- Queries: `Get{Noun}Query` (e.g. `GetRobotStatusQuery`)
- Domain Events: `{Noun}{PastTense}Event` (e.g. `RouteCompletedEvent`)
- Hangfire Jobs: `{Verb}{Noun}Job` (e.g. `TrackGpsProgressJob`)
- DTOs: `{Noun}Dto` — never expose domain entities to API responses.

### File Organisation
- One class per file, named to match.
- Commands and their handlers co-located in `Commands/{Name}/`.
- Queries and their handlers co-located in `Queries/{Name}/`.

### Domain Events
1. Raised inside aggregate methods (e.g. `Robot.StartFieldMarking()`).
2. Collected in `AggregateRoot._domainEvents`.
3. Dispatched by `AppDbContext.SaveChangesAsync` **after** successful persistence.
4. Handled by `INotificationHandler<T>` implementations in TT.Application.
5. Handlers enqueue Hangfire jobs — they do NOT call domain methods directly.

---

## Available Agents

See `.github/agents.md` for the `Architect Agent`, `Domain Expert Agent`, and `Infrastructure Agent` skill sets.
