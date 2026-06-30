using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TT.Application.Abstractions;
using TT.Application.EventHandlers;
using TT.Infrastructure.Jobs;
using TT.Infrastructure.Persistence;
using TT.Infrastructure.Services;

namespace TT.Infrastructure;

/// <summary>
/// Extension method that wires all Infrastructure dependencies into the DI container.
/// Called from TT.Api/Program.cs — the only place that knows about Infrastructure internals.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // EF Core — InMemory for demo; swap with UseSqlServer/UseNpgsql in production
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TT.BackendTransformation"));

        services.AddScoped<IRobotRepository, RobotRepository>();

        // Event log service — singleton so all handlers share the same SSE channel
        services.AddSingleton<EventLogService>();
        services.AddSingleton<IEventLogger>(sp => sp.GetRequiredService<EventLogService>());

        // Hangfire — InMemory storage for demo
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseInMemoryStorage());

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 4;
            options.Queues = ["default"];
        });

        // Register Hangfire jobs as transient services (resolved by Hangfire's IoC integration)
        services.AddTransient<TrackGpsProgressJob>();
        services.AddTransient<CompleteRouteJob>();
        services.AddTransient<PushStatisticsJob>();
        services.AddTransient<SendNotificationJob>();

        // Register job interfaces so Application event handlers can inject them
        services.AddTransient<ITrackGpsProgressJob, TrackGpsProgressJob>();
        services.AddTransient<IPushStatisticsJob, PushStatisticsJob>();

        return services;
    }
}
