using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wallabee.Core.Interfaces;
using Wallabee.Telemetry.Configuration;
namespace Wallabee.Telemetry.Retry;
public sealed class TelemetryDispatcher(ITelemetryQueue queue, ITelemetryPublisher publisher, IOptions<TelemetryOptions> options, ILogger<TelemetryDispatcher> logger)
{
    private static readonly int[] Delays = [2,5,15,30,60];
    private readonly SemaphoreSlim gate = new(1,1);
    public static TimeSpan RetryDelay(int attempt) => TimeSpan.FromSeconds(Delays[Math.Clamp(attempt, 0, Delays.Length - 1)]);
    public async Task DispatchAsync(CancellationToken ct)
    {
        if (!options.Value.Enabled) return;
        await gate.WaitAsync(ct);
        try
        {
            foreach (var item in await queue.GetDueAsync(options.Value.BatchSize, DateTimeOffset.UtcNow, ct))
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    if (item.Attempts > 0) logger.LogInformation("TelemetryRetry {EventId} {Attempt}", item.EventId, item.Attempts + 1);
                    await (item.EventType switch {
                        "scan" => publisher.PublishScanResultAsync(item, ct),
                        "benchmark" => publisher.PublishBenchmarkAsync(item, ct),
                        "round" => publisher.PublishRoundSummaryAsync(item, ct),
                        "system" => publisher.PublishSystemMetricsAsync(item, ct),
                        _ => throw new InvalidDataException("Unknown telemetry event type.") });
                    await queue.AcknowledgeAsync(item.EventId, ct);
                    logger.LogInformation("TelemetryPublished {EventId}", item.EventId);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException or InvalidDataException)
                {
                    // Do not persist response bodies, hostnames, credentials or raw exception text.
                    await queue.FailAsync(item.EventId, ex.GetType().Name, DateTimeOffset.UtcNow + RetryDelay(item.Attempts), !options.Value.RetryEnabled || item.Attempts + 1 >= options.Value.MaxAttempts, ct);
                    logger.LogWarning("TelemetryFailed {EventId} {ErrorType}", item.EventId, ex.GetType().Name);
                }
            }
        }
        finally { gate.Release(); }
    }
}
