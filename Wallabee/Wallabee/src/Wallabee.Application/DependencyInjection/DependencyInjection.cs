using Microsoft.Extensions.DependencyInjection;
using Wallabee.Application.Quarantine.Services;
using Wallabee.Application.Scanning.Services;

namespace Wallabee.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWallabeeApplication(this IServiceCollection services)
    {
        services.AddScoped<DetectionPipeline>();
        services.AddScoped<ScanFileService>();
        services.AddScoped<ScanDirectoryService>();
        services.AddScoped<QuarantineApplicationService>();
        return services;
    }
}