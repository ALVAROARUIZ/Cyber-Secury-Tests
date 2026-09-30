using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class ExperimentRoundEntity
{
    public Guid RoundId { get; set; }
    public Guid ExperimentId { get; set; }
    public string? ExternalRoundId { get; set; }
    public int RoundNumber { get; set; }
    public int? GenerationNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int SamplesGenerated { get; set; }
    public int SamplesScanned { get; set; }
    public int SamplesDetected { get; set; }
    public int SamplesMissed { get; set; }
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
    public DateTime CreatedAt { get; set; }
}
