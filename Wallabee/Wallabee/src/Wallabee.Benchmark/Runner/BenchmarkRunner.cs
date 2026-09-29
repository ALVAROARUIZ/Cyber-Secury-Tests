using Wallabee.Benchmark.Models;
using Wallabee.Benchmark.Statistics;
using Wallabee.Core.Common;
using Wallabee.Core.Interfaces;

namespace Wallabee.Benchmark.Runner;

public sealed class BenchmarkRunner(IFileScanner fileScanner)
{
    public async Task<BenchmarkResult> RunAsync(string datasetDirectory, CancellationToken cancellationToken = default)
    {
        var results = await fileScanner.ScanDirectoryAsync(datasetDirectory, cancellationToken).ConfigureAwait(false);
        var performance = BenchmarkMetricCalculator.CalculatePerformanceMetrics(results.Select(result => result.Duration), results.Sum(result => result.FileSize));
        var detected = results.Count(result => result.RiskScore > 0);
        var detection = BenchmarkMetricCalculator.CalculateDetectionMetrics(detected, results.Count - detected, 0, 0);
        var run = new BenchmarkRun(Guid.NewGuid(), DateTimeOffset.UtcNow, Path.GetFileName(datasetDirectory), results.Sum(result => result.FileSize), WallabeeVersion.Current);
        return new BenchmarkResult(run, performance, detection, null, []);
    }
}