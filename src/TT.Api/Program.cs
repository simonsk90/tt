using System.Text.Json;
using Hangfire;
using MediatR;
using TT.Application.Commands.StartFieldMarking;
using TT.Application.Queries.GetRobotStatus;
using TT.Infrastructure;
using TT.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(StartFieldMarkingCommand).Assembly));

builder.Services.AddInfrastructure();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
    app.UseHangfireDashboard("/hangfire");

// POST /api/robots/start — launches the full field marking pipeline
app.MapPost("/api/robots/start", async (IMediator mediator) =>
{
    try
    {
        var robotId = await mediator.Send(new StartFieldMarkingCommand());
        return Results.Ok(new { robotId, message = "Field marking started." });
    }
    catch (Exception ex)
    {
        return Results.UnprocessableEntity(new { error = ex.Message });
    }
});

// GET /api/robots/{id} — lightweight status query
app.MapGet("/api/robots/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new GetRobotStatusQuery(id));
    return result is null ? Results.NotFound() : Results.Ok(result);
});

// GET /api/events — SSE stream: frontend terminal subscribes here
app.MapGet("/api/events", async (EventLogService eventLog, HttpResponse response, CancellationToken ct) =>
{
    response.Headers.ContentType = "text/event-stream";
    response.Headers.CacheControl = "no-cache";
    response.Headers["X-Accel-Buffering"] = "no";

    var (history, reader, channel) = eventLog.Subscribe();
    try
    {
        foreach (var entry in history)
            await WriteEventAsync(response, entry, ct);

        await foreach (var entry in reader.ReadAllAsync(ct))
            await WriteEventAsync(response, entry, ct);
    }
    finally
    {
        eventLog.Unsubscribe(channel);
    }
});

// GET /api/events/history — JSON snapshot fallback
app.MapGet("/api/events/history", (EventLogService eventLog) =>
    Results.Ok(eventLog.GetHistory()));

app.Run();

static async Task WriteEventAsync(HttpResponse response, EventLogEntry entry, CancellationToken ct)
{
    var json = JsonSerializer.Serialize(entry,
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    await response.WriteAsync($"data: {json}\n\n", ct);
    await response.Body.FlushAsync(ct);
}
