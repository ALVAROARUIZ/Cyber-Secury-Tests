namespace Wallabee.Core.Configuration;

public sealed class WallabeeOptions
{
    public const string SectionName = "Wallabee";
    public long MaximumFileSize { get; init; } = 100 * 1024 * 1024;
    public int MaximumParallelScans { get; init; } = 4;
    public int ScanBufferSize { get; init; } = 81920;
    public string LabRoot { get; init; } = "lab";
    public string Version { get; init; } = "0.1.0";
}

public sealed class ScannerOptions
{
    public const string SectionName = "Scanner";
    public bool AllowDirectoryScanning { get; init; } = true;
}

public sealed class QuarantineOptions
{
    public const string SectionName = "Quarantine";
    public string Directory { get; init; } = "data/quarantine";
    public bool AutoQuarantine { get; init; }
    public bool AllowRestore { get; init; }
}

public sealed class BenchmarkOptions
{
    public const string SectionName = "Benchmark";
    public bool Enabled { get; init; } = true;
    public bool RecordSystemMetrics { get; init; }
}
public sealed class PersistenceOptions
{
    public string TestHostAlias { get; init; } = "local-lab";
}
public sealed class ScoringOptions
{
    public int Low { get; init; } = 20;
    public int Medium { get; init; } = 50;
    public int High { get; init; } = 75;
    public int Critical { get; init; } = 90;
}
public sealed class HeuristicOptions
{
    public string[] KnownHashes { get; init; } = [];
    public double HighEntropyThreshold { get; init; } = 7.8;
    public long UnusualSizeBytes { get; init; } = 50 * 1024 * 1024;
}
