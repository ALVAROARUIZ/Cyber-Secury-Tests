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
    public string Directory { get; init; } = "lab/quarantine";
    public bool AllowRestore { get; init; }
}

public sealed class BenchmarkOptions
{
    public const string SectionName = "Benchmark";
    public bool Enabled { get; init; } = true;
    public bool RecordSystemMetrics { get; init; }
}