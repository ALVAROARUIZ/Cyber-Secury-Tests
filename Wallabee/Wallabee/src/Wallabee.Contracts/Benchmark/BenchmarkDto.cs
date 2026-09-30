namespace Wallabee.Contracts.Benchmark;
public record BenchmarkDto
{
    public Guid BenchmarkId { get; init; }
    public string? ExperimentId { get; init; }
    public string? RoundId { get; init; }
    public string WallabeeVersion { get; init; } = "";
    public string? HoneyBadgerVersion { get; init; }
    public int SamplesScanned { get; init; }
    public int SamplesDetected { get; init; }
    public int SamplesMissed { get; init; }
    public int LabeledSamples { get; init; }
    public int TruePositives { get; init; }
    public int TrueNegatives { get; init; }
    public int FalsePositives { get; init; }
    public int FalseNegatives { get; init; }
    public double? DetectionRate { get; init; }
    public double? EvasionRate { get; init; }
    public double? Precision { get; init; }
    public double? Recall { get; init; }
    public double? F1Score { get; init; }
    public double? FalsePositiveRate { get; init; }
    public double? FalseNegativeRate { get; init; }
    public double TotalScanTimeMs { get; init; }
    public double AverageScanMs { get; init; }
    public double P50ScanMs { get; init; }
    public double P95ScanMs { get; init; }
    public double P99ScanMs { get; init; }
    public double FilesPerSecond { get; init; }
    public double BytesPerSecond { get; init; }
    public double MbPerSecond { get; init; }
    public double? CpuAverage { get; init; }
    public double? CpuPeak { get; init; }
    public double? RamAverageMb { get; init; }
    public double? RamPeakMb { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
}
