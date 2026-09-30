using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class ExperimentEntity
{
    public Guid ExperimentId { get; set; }
    public string? ExternalExperimentId { get; set; }
    public string ExperimentName { get; set; } = null!;
    public string? Description { get; set; }
    public int WallabeeVersionId { get; set; }
    public int? HoneyBadgerVersionId { get; set; }
    public Guid? DatasetId { get; set; }
    public int? TestHostId { get; set; }
    public string? ExperimentType { get; set; }
    public string Status { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? RandomSeed { get; set; }
    public int? TotalRounds { get; set; }
    public int? TotalSamples { get; set; }
    public string? ConfigurationJson { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
