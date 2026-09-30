using System.ComponentModel.DataAnnotations;
namespace Wallabee.Contracts.Requests;
public class ExperimentRequest
{
    [MaxLength(200)] public string? ExperimentId { get; init; }
    [MaxLength(200)] public string? RoundId { get; init; }
    [MaxLength(200)] public string? SampleId { get; init; }
    [MaxLength(100)] public string? HoneyBadgerVersion { get; init; }
    public bool RetainSample { get; init; }
    [RegularExpression("Regression|FalseNegative|FalsePositive|Research")] public string? RetainReason { get; init; }
}
public class ScanFileRequest : ExperimentRequest { [Required] public string FilePath { get; init; } = ""; }
public class DirectoryScanRequest : ExperimentRequest { [Required] public string DirectoryPath { get; init; } = ""; }
public class BenchmarkRunRequest : ExperimentRequest
{
    [Required] public string DatasetDirectory { get; init; } = "";
    // Keys are dataset-relative paths; true = expected positive, false = expected clean.
    public Dictionary<string, bool>? ExpectedLabels { get; init; }
    [Range(0, int.MaxValue)] public int? SamplesGenerated { get; init; }
}
