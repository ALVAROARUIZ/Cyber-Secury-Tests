using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class TelemetryEventEntity
{
    public long TelemetryEventId { get; set; }
    public Guid EventId { get; set; }
    public Guid? ExperimentId { get; set; }
    public Guid? RoundId { get; set; }
    public Guid? SampleId { get; set; }
    public Guid? ScanId { get; set; }
    public Guid? BenchmarkId { get; set; }
    public string SourceComponent { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public string? Severity { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? Message { get; set; }
    public string? PayloadJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public string State { get; set; } = null!;
    public int Attempts { get; set; }
    public DateTime NextAttemptUtc { get; set; }
    public string? LastError { get; set; }
}
