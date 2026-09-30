using Wallabee.Core.Models;
using Wallabee.Core.Enums;
namespace Wallabee.Core.Interfaces;
public interface IPolicyEngine { ResponseAction Evaluate(DetectionLevel level); }
public sealed record ScanFilter(string? Sha256 = null, string? ExperimentId = null, string? RoundId = null, string? SampleId = null, int Limit = 100, int Offset = 0);
public interface IScanResultRepository
{
    // Result, indicators and outbox event commit in ONE database transaction.
    Task SaveAsync(FileScanResult result, TelemetryEvent telemetry, CancellationToken ct = default);
    Task<FileScanResult?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FileScanResult>> FindAsync(ScanFilter filter, CancellationToken ct = default);
}
public sealed record TelemetryEvent(Guid EventId, string EventType, string Payload, Guid? ScanId = null, int Attempts = 0);
public sealed record TelemetryStatus(int Pending, int Failed, int Published);
public interface ITelemetryQueue
{
    Task EnqueueAsync(TelemetryEvent item, CancellationToken ct = default);
    Task<IReadOnlyList<TelemetryEvent>> GetDueAsync(int limit, DateTimeOffset now, CancellationToken ct = default);
    Task AcknowledgeAsync(Guid id, CancellationToken ct = default);
    Task FailAsync(Guid id, string error, DateTimeOffset nextAttempt, bool exhausted, CancellationToken ct = default);
    Task<TelemetryStatus> GetStatusAsync(CancellationToken ct = default);
    Task RetryAsync(CancellationToken ct = default);
}
public interface ITelemetryPublisher
{
    Task PublishScanResultAsync(TelemetryEvent item, CancellationToken ct = default);
    Task PublishBenchmarkAsync(TelemetryEvent item, CancellationToken ct = default);
    Task PublishRoundSummaryAsync(TelemetryEvent item, CancellationToken ct = default);
    Task PublishSystemMetricsAsync(TelemetryEvent item, CancellationToken ct = default);
}
public interface IResultDocumentRepository
{
    Task SaveAsync(Guid id, string kind, string json, IReadOnlyList<TelemetryEvent> events, CancellationToken ct = default);
    Task<string?> GetAsync(Guid? id, string kind, CancellationToken ct = default);
}
public interface IQuarantineRepository
{
    Task SaveAsync(QuarantineEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<QuarantineEntry>> ListAsync(CancellationToken ct = default);
}

public interface ISampleLabelRepository
{
    Task SetLabelsAsync(IReadOnlyDictionary<Guid, bool> scanLabels, CancellationToken ct = default);
}
