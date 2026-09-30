using Wallabee.Benchmark.Models;

namespace Wallabee.Benchmark.Statistics;

public static class BenchmarkMetricCalculator
{
    public static DetectionMetrics CalculateDetectionMetrics(int truePositives, int trueNegatives, int falsePositives, int falseNegatives)
    {
        var precision = Divide(truePositives, truePositives + falsePositives);
        var recall = Divide(truePositives, truePositives + falseNegatives);
        return new DetectionMetrics(truePositives, trueNegatives, falsePositives, falseNegatives, precision, recall, Divide(2 * precision * recall, precision + recall), recall, Divide(falsePositives, falsePositives + trueNegatives), Divide(falseNegatives, falseNegatives + truePositives));
    }

    public static PerformanceMetrics CalculatePerformanceMetrics(IEnumerable<TimeSpan> latencies, long totalBytes, TimeSpan? wallTime = null)
    {
        var samples = latencies.Order().ToArray();
        var total = wallTime ?? TimeSpan.FromTicks(samples.Sum(value => value.Ticks));
        var seconds = Math.Max(total.TotalSeconds, double.Epsilon);
        return new PerformanceMetrics(total, samples.Length, total.TotalSeconds <= 0 ? 0 : samples.Length / seconds, total.TotalSeconds <= 0 ? 0 : totalBytes / seconds, total.TotalSeconds <= 0 ? 0 : totalBytes / 1024d / 1024d / seconds, samples.Length == 0 ? TimeSpan.Zero : TimeSpan.FromTicks((long)samples.Average(value => value.Ticks)), Percentile(samples, 0.5), Percentile(samples, 0.95), Percentile(samples, 0.99));
    }

    private static double Divide(double numerator, double denominator) => denominator == 0 ? 0 : numerator / denominator;
    private static TimeSpan Percentile(IReadOnlyList<TimeSpan> samples, double percentile) => samples.Count == 0 ? TimeSpan.Zero : samples[Math.Min(samples.Count - 1, (int)Math.Ceiling(samples.Count * percentile) - 1)];
}