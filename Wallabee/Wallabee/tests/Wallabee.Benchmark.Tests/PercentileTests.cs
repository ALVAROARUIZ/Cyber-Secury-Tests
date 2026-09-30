using Wallabee.Benchmark.Statistics;
namespace Wallabee.Benchmark.Tests;
public sealed class PercentileTests
{
    [Fact]
    public void NearestRankPercentilesAndWallClockThroughput()
    {
        var result = BenchmarkMetricCalculator.CalculatePerformanceMetrics(Enumerable.Range(1, 100).Select(i => TimeSpan.FromMilliseconds(i)), 1048576, TimeSpan.FromSeconds(2));
        Assert.Equal(50, result.MedianLatency.TotalMilliseconds);
        Assert.Equal(95, result.P95Latency.TotalMilliseconds);
        Assert.Equal(99, result.P99Latency.TotalMilliseconds);
        Assert.Equal(50, result.FilesPerSecond); Assert.Equal(.5, result.MegabytesPerSecond);
    }
    [Fact]
    public void EmptyDatasetIsFinite()
    {
        var result = BenchmarkMetricCalculator.CalculatePerformanceMetrics([], 0);
        Assert.Equal(0, result.FilesPerSecond); Assert.Equal(TimeSpan.Zero, result.MeanLatency); Assert.Equal(TimeSpan.Zero, result.P99Latency);
    }
    [Fact]
    public void ConfusionMatrixHandlesFalseResults()
    {
        var result = BenchmarkMetricCalculator.CalculateDetectionMetrics(3, 2, 1, 2);
        Assert.Equal(.75, result.Precision); Assert.Equal(.6, result.Recall); Assert.Equal(.4, result.FalseNegativeRate);
        Assert.Equal(1d/3, result.FalsePositiveRate, 10);
    }
}
