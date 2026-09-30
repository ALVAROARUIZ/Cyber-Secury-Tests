using Wallabee.Core.Interfaces;
namespace Wallabee.Application.Telemetry;
public sealed class TelemetryService(ITelemetryQueue queue)
{
    public Task<TelemetryStatus> GetStatusAsync(CancellationToken ct) => queue.GetStatusAsync(ct);
    public Task RetryAsync(CancellationToken ct) => queue.RetryAsync(ct);
}
