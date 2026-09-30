using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wallabee.Core.Interfaces;
using Wallabee.Telemetry.Configuration;
using Wallabee.Telemetry.Publisher;
using Wallabee.Telemetry.Retry;
using Wallabee.Telemetry.BackgroundServices;
namespace Wallabee.Telemetry.DependencyInjection;
public static class DependencyInjection
{
    public static IServiceCollection AddWallabeeTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TelemetryOptions>().Bind(configuration.GetSection("Telemetry"))
            .Validate(o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out var u) && (u.Scheme == "https" || u.IsLoopback), "Telemetry requires HTTPS or loopback HTTP.")
            .Validate(o => o.BatchSize is > 0 and <= 1000 && o.MaxAttempts is > 0 and <= 100 && o.PollSeconds is > 0 and <= 300 && o.TimeoutSeconds is > 0 and <= 120, "Invalid telemetry limits.").ValidateOnStart();
        services.AddHttpClient<ITelemetryPublisher, HttpTelemetryPublisher>((provider, client) =>
        {
            var o = provider.GetRequiredService<IOptions<TelemetryOptions>>().Value;
            client.BaseAddress = new Uri(o.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(o.TimeoutSeconds);
        });
        services.AddSingleton<TelemetryDispatcher>();
        services.AddHostedService<TelemetryBackgroundPublisher>();
        return services;
    }
}
