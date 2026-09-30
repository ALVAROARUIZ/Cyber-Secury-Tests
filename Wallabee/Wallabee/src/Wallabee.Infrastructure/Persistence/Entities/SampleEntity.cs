using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class SampleEntity
{
    public Guid SampleId { get; set; }
    public string? ExternalSampleId { get; set; }
    public Guid? ExperimentId { get; set; }
    public Guid? RoundId { get; set; }
    public Guid? ParentSampleId { get; set; }
    public string? SampleFamily { get; set; }
    public string? SampleType { get; set; }
    public string? GroundTruthLabel { get; set; }
    public int? GenerationNumber { get; set; }
    public string? FileName { get; set; }
    public string? RelativePath { get; set; }
    public string Sha256 { get; set; } = null!;
    public long SizeBytes { get; set; }
    public decimal? Entropy { get; set; }
    public string? MutationStrategy { get; set; }
    public string? MutationParametersJson { get; set; }
    public decimal? FitnessScore { get; set; }
    public bool RetainSample { get; set; }
    public string? RetainReason { get; set; }
    public string? ExternalStorageReference { get; set; }
    public DateTime CreatedAt { get; set; }
}
