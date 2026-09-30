using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class ScanEntity
{
    public Guid ScanId { get; set; }
    public Guid? SampleId { get; set; }
    public Guid? ExperimentId { get; set; }
    public Guid? RoundId { get; set; }
    public int WallabeeVersionId { get; set; }
    public int? TestHostId { get; set; }
    public string Sha256 { get; set; } = null!;
    public string? FileName { get; set; }
    public long FileSizeBytes { get; set; }
    public decimal RiskScore { get; set; }
    public DetectionLevel DetectionLevel { get; set; }
    public bool WasDetected { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public decimal DurationMs { get; set; }
    public decimal? BytesPerSecond { get; set; }
    public bool ErrorOccurred { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? HoneyBadgerVersionId { get; set; }
    public string? ExternalExperimentId { get; set; }
    public string? ExternalRoundId { get; set; }
    public string? ExternalSampleId { get; set; }
    public string ResponseAction { get; set; } = null!;
    public bool RetainSample { get; set; }
    public string? RetainReason { get; set; }
    public bool TelemetryPublished { get; set; }
    public WallabeeVersionEntity WallabeeVersion { get; set; } = null!;
    public HoneyBadgerVersionEntity? HoneyBadgerVersion { get; set; }
    public List<ThreatIndicatorEntity> Indicators { get; set; } = [];
}
