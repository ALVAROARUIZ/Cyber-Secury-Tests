using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wallabee.Telemetry.Configuration;
using Wallabee.Telemetry.Retry;
namespace Wallabee.Telemetry.BackgroundServices;
public sealed class TelemetryBackgroundPublisher(TelemetryDispatcher dispatcher, IOptions<TelemetryOptions> options, ILogger<TelemetryBackgroundPublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await dispatcher.DispatchAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError("Telemetry queue unavailable {ErrorType}", ex.GetType().Name); }
            try { await Task.Delay(TimeSpan.FromSeconds(options.Value.PollSeconds), stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
