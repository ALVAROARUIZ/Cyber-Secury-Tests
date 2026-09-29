using Wallabee.Benchmark.Statistics;

namespace Wallabee.Benchmark.Tests;

public sealed class BenchmarkMetricCalculatorTests
{
    [Fact]
    public void CalculateDetectionMetrics_ComputesPrecisionRecallAndF1()
    {
        var result = BenchmarkMetricCalculator.CalculateDetectionMetrics(8, 10, 2, 2);

        Assert.Equal(0.8, result.Precision, 5);
        Assert.Equal(0.8, result.Recall, 5);
        Assert.Equal(0.8, result.F1Score, 5);
    }
}