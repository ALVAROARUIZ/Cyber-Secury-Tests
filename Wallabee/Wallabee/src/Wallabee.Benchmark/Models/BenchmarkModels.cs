namespace Wallabee.Benchmark.Models;

public sealed record BenchmarkRun(Guid BenchmarkId, DateTimeOffset Timestamp, string DatasetId, long DatasetSize, string WallabeeVersion);
public sealed record BenchmarkResult(BenchmarkRun Run, PerformanceMetrics Performance, DetectionMetrics Detection, SystemMetrics? System, IReadOnlyCollection<RoundMetrics> Rounds);
public sealed record PerformanceMetrics(TimeSpan TotalScanTime, int FilesScanned, double FilesPerSecond, double BytesPerSecond, double MegabytesPerSecond, TimeSpan MeanLatency, TimeSpan MedianLatency, TimeSpan P95Latency, TimeSpan P99Latency);
public sealed record DetectionMetrics(int TruePositives, int TrueNegatives, int FalsePositives, int FalseNegatives, double Precision, double Recall, double F1Score, double DetectionRate, double FalsePositiveRate, double FalseNegativeRate);
public sealed record SystemMetrics(double CpuUsagePercent, long PeakMemoryBytes, long AverageMemoryBytes);
public sealed record RoundMetrics(int RoundNumber, int SyntheticSamplesGenerated, int SamplesDetected, int SamplesMissed, double EvasionRate, double DetectionImprovement);