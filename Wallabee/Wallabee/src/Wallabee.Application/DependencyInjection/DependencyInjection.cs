using Microsoft.Extensions.DependencyInjection;
using Wallabee.Application.Quarantine.Services;
using Wallabee.Application.Scanning.Services;

namespace Wallabee.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWallabeeApplication(this IServiceCollection services)
    {
        services.AddSingleton<Wallabee.Application.Monitoring.ActivityMonitor>();
        services.AddScoped<DetectionPipeline>();
        services.AddScoped<ScanQueryService>();
        services.AddScoped<Wallabee.Application.Telemetry.TelemetryService>();
        services.AddScoped<ScanFileService>();
        services.AddScoped<ScanDirectoryService>();
        services.AddScoped<QuarantineApplicationService>();
        return services;
    }
}