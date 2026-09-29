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
    int RiskScore,
    DetectionLevel DetectionLevel,
    IReadOnlyCollection<ThreatIndicator> Indicators,
    TimeSpan Duration,
    string? Error = null);

public sealed record DetectionResult(
    DetectionMethod Method,
    DetectionLevel Level,
    ThreatCategory Category,
    IReadOnlyCollection<ThreatIndicator> Indicators);

public sealed record ThreatIndicator(
    string Source,
    string Name,
    string Description,
    int Weight,
    ThreatCategory Category = ThreatCategory.Unknown);

public sealed record RiskAssessment(int Score, DetectionLevel Level, IReadOnlyCollection<ThreatIndicator> Indicators);

public sealed record QuarantineEntry(
    Guid Id,
    string OriginalPath,
    string QuarantinedPath,
    DateTimeOffset QuarantinedAt,
    string Sha256);

public sealed record FileMetadata(string FullPath, string Name, long Length, DateTimeOffset LastModifiedAt);

public sealed record HashResult(string Algorithm, string Value);

public sealed record FileContext(FileMetadata Metadata, HashResult? Hash = null);