using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class DetectionConfusionMatrixEntity
{
    public long DetectionConfusionMatrixId { get; set; }
    public Guid ExperimentId { get; set; }
    public Guid? RoundId { get; set; }
    public Guid? BenchmarkId { get; set; }
    public int TruePositives { get; set; }
    public int TrueNegatives { get; set; }
    public int FalsePositives { get; set; }
    public int FalseNegatives { get; set; }
    public decimal? PrecisionScore { get; set; }
    public decimal? RecallScore { get; set; }
    public decimal? SpecificityScore { get; set; }
    public decimal? F1Score { get; set; }
    public decimal? AccuracyScore { get; set; }
    public DateTime CreatedAt { get; set; }
}
