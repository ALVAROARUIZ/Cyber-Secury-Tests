using Wallabee.Core.Enums;

namespace Wallabee.Core.Models;

public sealed record ScanRequest(string Path, bool IncludeSubdirectories = false);

public sealed record ScanResult(
    Guid ScanId,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    ScanStatus Status,
    int FilesScanned,
    int ThreatsDetected,
    int FilesClean,
    IReadOnlyCollection<string> Errors,
    IReadOnlyCollection<FileScanResult> Files)
{
    public TimeSpan? Duration => CompletedAt - StartedAt;
}

public sealed record FileScanResult(
    string FilePath,
    long FileSize,
    string? Sha256,
    double RiskScore,
    DetectionLevel DetectionLevel,
    IReadOnlyCollection<ThreatIndicator> Indicators,
    TimeSpan Duration,
    string? Error = null)
{
    public Guid ScanId { get; init; } = Guid.NewGuid();
    public string FileName => System.IO.Path.GetFileName(FilePath);
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
    public double DurationMs => Duration.TotalMilliseconds;
    public string WallabeeVersion { get; init; } = Common.WallabeeVersion.Current;
    public string? ExperimentId { get; init; }
    public string? RoundId { get; init; }
    public string? SampleId { get; init; }
    public string? HoneyBadgerVersion { get; init; }
    public bool RetainSample { get; init; }
    public string? RetainReason { get; init; }
    public ResponseAction ResponseAction { get; init; }
}

public sealed record DetectionResult(
    DetectionMethod Method,
    DetectionLevel Level,
    ThreatCategory Category,
    IReadOnlyCollection<ThreatIndicator> Indicators);

public sealed record ThreatIndicator(
    string Source,
    string Name,
    string Description,
    double Weight,
    ThreatCategory Category = ThreatCategory.Unknown)
{
    public Guid IndicatorId { get; init; } = Guid.NewGuid();
    public Guid ScanId { get; init; }
}

public sealed record RiskAssessment(double Score, DetectionLevel Level, IReadOnlyCollection<ThreatIndicator> Indicators);

public sealed record QuarantineEntry(
    Guid Id,
    string OriginalPath,
    string QuarantinedPath,
    DateTimeOffset QuarantinedAt,
    string Sha256)
{
    public string Reason { get; init; } = "Manual";
    public DateTimeOffset? RestoredAt { get; init; }
    public string State { get; init; } = "Pending";
}

public sealed record FileMetadata(string FullPath, string Name, long Length, DateTimeOffset LastModifiedAt);

public sealed record HashResult(string Algorithm, string Value);

public sealed record FileContext(FileMetadata Metadata, HashResult? Hash = null, StaticAnalysis? Analysis = null);
public sealed record ExperimentContext(string? ExperimentId = null, string? RoundId = null, string? SampleId = null, string? HoneyBadgerVersion = null, bool RetainSample = false, string? RetainReason = null);
public sealed record StaticAnalysis(double Entropy, double DominantByteRatio, string? DetectedExtension, long BytesAnalyzed);
public sealed record SystemMetrics(double CpuAverage, double CpuPeak, double RamAverageMb, double RamPeakMb, DateTimeOffset Timestamp);
