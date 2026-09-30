using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class BenchmarkResultEntity
{
    public Guid BenchmarkId { get; set; }
    public Guid? ExperimentId { get; set; }
    public Guid? RoundId { get; set; }
    public int WallabeeVersionId { get; set; }
    public int? HoneyBadgerVersionId { get; set; }
    public int? TestHostId { get; set; }
    public string BenchmarkType { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal? DurationMs { get; set; }
    public int SamplesScanned { get; set; }
    public int? SamplesDetected { get; set; }
    public int? SamplesMissed { get; set; }
    public int? TruePositives { get; set; }
    public int? TrueNegatives { get; set; }
    public int? FalsePositives { get; set; }
    public int? FalseNegatives { get; set; }
    public decimal? DetectionRate { get; set; }
    public decimal? EvasionRate { get; set; }
    public decimal? PrecisionScore { get; set; }
    public decimal? RecallScore { get; set; }
    public decimal? F1Score { get; set; }
    public decimal? AverageRiskScore { get; set; }
    public decimal? AverageScanMs { get; set; }
    public decimal? P50ScanMs { get; set; }
    public decimal? P95ScanMs { get; set; }
    public decimal? P99ScanMs { get; set; }
    public decimal? FilesPerSecond { get; set; }
    public decimal? MbPerSecond { get; set; }
    public decimal? CpuAveragePercent { get; set; }
    public decimal? CpuPeakPercent { get; set; }
    public decimal? MemoryAverageMb { get; set; }
    public decimal? MemoryPeakMb { get; set; }
    public string? ConfigurationJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LabeledSamples { get; set; }
    public decimal? FalsePositiveRate { get; set; }
    public decimal? FalseNegativeRate { get; set; }
    public decimal BytesPerSecond { get; set; }
    public int? SamplesGenerated { get; set; }
    public string? ExternalExperimentId { get; set; }
    public string? ExternalRoundId { get; set; }
    public WallabeeVersionEntity WallabeeVersion { get; set; } = null!;
    public HoneyBadgerVersionEntity? HoneyBadgerVersion { get; set; }
}
