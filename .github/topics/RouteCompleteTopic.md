# RouteComplete Domain — Deep-Dive Reference

> **AI AGENTS: Read this file in full before modifying `Robot.cs`, `RouteCompletedEvent.cs`,
> `FieldMarkingStartedEventHandler.cs`, `RouteCompletedEventHandler.cs`,
> `TrackGpsProgressJob.cs`, `PushStatisticsJob.cs`, or `SendNotificationJob.cs`.**

---

## 1. Domain Concept

**Field Marking** is the process by which a TT robot physically marks a sports field
(pitch, court, track) using paint or chalk dispensed as it follows a GPS-defined route.

A **Route** represents the complete GPS path the robot must travel to mark one field.
The lifecycle of a route is:

```
Not Started → Working → Completed
```

The transition from `Not Started → Working` is triggered by `StartFieldMarking()`.
The transition from `Working → Completed` is triggered by `CompleteRoute()`.

There is no partial completion or pause state in this version of the domain.

---

## 2. Business Invariants

These rules are enforced inside the `Robot` aggregate. **They must never be relaxed or moved
to a service or handler.** Violation of an invariant raises `DomainException`.

### StartFieldMarking() Invariants

| # | Invariant | DomainException message |
|---|-----------|------------------------|
| 1 | Robot MUST NOT be in `Working` status | "Robot '{Name}' is already performing a field marking operation." |
| 2 | Battery level MUST be ≥ 20% | "Insufficient battery ({level}%). At least 20% is required to start field marking." |

**Rationale for #2:** A robot that starts with less than 20% battery risks running out mid-route,
leaving the field half-marked. This would require manual cleanup and field closure.

### CompleteRoute() Invariants

| # | Invariant | DomainException message |
|---|-----------|------------------------|
| 1 | Robot MUST be in `Working` status | "Cannot complete route for robot '{Name}' — route was never started." |
| 2 | `LastKnownPosition` MUST NOT be null | "Cannot complete route for robot '{Name}' — GPS data is unavailable." |

**Rationale for #1:** Completing a route that was never started is a logical impossibility.
This prevents ghost completions from stale background jobs.

**Rationale for #2:** The final GPS position is stored in `RouteCompletedEvent` and used
downstream by the statistics system and field management platform. A route without
a final position cannot be costed, billed, or logged.

---

## 3. State Diagram

```
           StartFieldMarking()
   Idle ─────────────────────────► Working
    ▲                                  │
    │         CompleteRoute()          │
    └──────────────────────────────────┘

   Error (reserved for future: GPS loss, obstacle collision, low-battery mid-route)
```

---

## 4. Complete Event Chain

This section describes the full lifecycle from API call to operator notification.

```
Browser
  │
  ├─ POST /api/robots/start
  │
  └─► [1] StartFieldMarkingCommand          (Application.Commands)
            │
            ├─► Robot.StartFieldMarking()   (Domain) — enforces invariants
            │         └─ Raises FieldMarkingStartedEvent
            │
            └─► AppDbContext.SaveChangesAsync()  (Infrastructure)
                      └─ Publishes FieldMarkingStartedEvent via MediatR

  [2] FieldMarkingStartedEventHandler       (Application.EventHandlers)
        └─► Enqueues TrackGpsProgressJob    (Hangfire)

  [3] TrackGpsProgressJob                   (Infrastructure.Jobs)
        ├─ Simulates GPS telemetry (async delays)
        └─► Enqueues CompleteRouteJob       (Hangfire)

  [4] CompleteRouteJob                      (Infrastructure.Jobs)
        └─► Sends CompleteRouteCommand      (via MediatR)
              └─► Robot.CompleteRoute()     (Domain) — enforces invariants
                        └─ Raises RouteCompletedEvent
              └─► AppDbContext.SaveChangesAsync()
                        └─ Publishes RouteCompletedEvent via MediatR

  [5] RouteCompletedEventHandler            (Application.EventHandlers)
        └─► Enqueues PushStatisticsJob      (Hangfire)

  [6] PushStatisticsJob                     (Infrastructure.Jobs)
        ├─ POSTs telemetry to cloud analytics endpoint
        └─► Enqueues SendNotificationJob    (Hangfire)

  [7] SendNotificationJob                   (Infrastructure.Jobs)
        └─ Sends operator push notification / webhook
           Logs "Job complete! 🚀" to EventLogService → SSE → Browser terminal
```

---

## 5. Domain Events Reference

### FieldMarkingStartedEvent

```csharp
record FieldMarkingStartedEvent(Guid RobotId, string RobotName, DateTimeOffset StartedAt)
    : DomainEvent(StartedAt);
```

- Raised by: `Robot.StartFieldMarking()`
- Consumed by: `FieldMarkingStartedEventHandler` (enqueues `TrackGpsProgressJob`)
- NOT persisted to any event store in this demo (outbox pattern: future work)

### RouteCompletedEvent

```csharp
record RouteCompletedEvent(
    Guid RobotId,
    string RobotName,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    GpsCoordinates FinalPosition) : DomainEvent(CompletedAt);
```

- Raised by: `Robot.CompleteRoute()`
- Consumed by: `RouteCompletedEventHandler` (enqueues `PushStatisticsJob`)
- `Duration` property computed as `CompletedAt - StartedAt`
- `FinalPosition` is the last GPS reading before the route ended

---

## 6. Value Objects

### GpsCoordinates

```csharp
record GpsCoordinates(double Latitude, double Longitude)
```

- Latitude: [-90, 90] — validated in constructor
- Longitude: [-180, 180] — validated in constructor
- Mapped via EF Core `OwnsOne` with shadow columns `GpsLatitude`, `GpsLongitude`

---

## 7. Robot Aggregate — Property Reference

| Property | Type | Mutated by |
|----------|------|------------|
| `Id` | `Guid` | `Robot.Create()` only |
| `Name` | `string` | `Robot.Create()` only |
| `Status` | `RobotStatus` | `StartFieldMarking()`, `CompleteRoute()` |
| `BatteryLevel` | `double` | `UpdateBatteryLevel()` |
| `LastKnownPosition` | `GpsCoordinates?` | `UpdateGpsPosition()` |
| `FieldMarkingStartedAt` | `DateTimeOffset?` | `StartFieldMarking()` |
| `RouteCompletedAt` | `DateTimeOffset?` | `CompleteRoute()` |
| `DomainEvents` | `IReadOnlyList<DomainEvent>` | `RaiseDomainEvent()`, cleared by Infrastructure |

---

## 8. Extension Points (Future Work)

| Area | Current | Suggested Extension |
|------|---------|---------------------|
| Error recovery | `RobotStatus.Error` defined, unused | Add `ReportError(string reason)` domain method |
| Mid-route battery | Not checked | Add battery telemetry subscription in `TrackGpsProgressJob` |
| Multi-robot | Single robot per command | Introduce `Fleet` aggregate or `IRobotFleetRepository` |
| Event persistence | In-memory SSE only | Implement outbox pattern with a `DomainEventLog` EF Core table |
| GPS fence validation | No boundary check | Add `FieldBoundary` value object and validate robot stays within it |
