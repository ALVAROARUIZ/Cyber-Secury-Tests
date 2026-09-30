using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wallabee.Application.Mapping;
using Wallabee.Application.Scanning.Services;
using Wallabee.Benchmark.Statistics;
using Wallabee.Benchmark.Profiling;
using Wallabee.Contracts.Benchmark;
using Wallabee.Contracts.Experiments;
using Wallabee.Contracts.Requests;
using Wallabee.Core.Common;
using Wallabee.Core.Configuration;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Enums;
namespace Wallabee.Benchmark.Runner;
public sealed class BenchmarkRunner(ScanDirectoryService scanner, IResultDocumentRepository repository, ISampleLabelRepository sampleLabels, IOptions<BenchmarkOptions> options, ILogger<BenchmarkRunner> logger, Wallabee.Application.Monitoring.ActivityMonitor? monitor = null)
{
    public Task<BenchmarkDto> RunAsync(string path, CancellationToken ct = default) => RunAsync(new BenchmarkRunRequest { DatasetDirectory = path }, ct);
    public async Task<BenchmarkDto> RunAsync(BenchmarkRunRequest request, CancellationToken ct = default)
    {
        if (!options.Value.Enabled) throw new InvalidOperationException("Benchmarking is disabled.");
        using var activity = monitor?.Begin("benchmark", Path.GetFileName(Path.TrimEndingDirectorySeparator(request.DatasetDirectory)), request.ExperimentId, ct);
        var id = Guid.NewGuid(); var started = DateTimeOffset.UtcNow;
        logger.LogInformation("BenchmarkStarted {BenchmarkId}", id);
        await using var profiler = options.Value.RecordSystemMetrics ? new ProcessProfiler() : null;
        var timer = Stopwatch.StartNew();
        var results = await scanner.ExecuteAsync(request.DatasetDirectory, request.ToContext(), ct);
        timer.Stop();
        activity?.Stage("Calculando métricas");
        var system = profiler is null ? null : await profiler.CompleteAsync();
        var performance = BenchmarkMetricCalculator.CalculatePerformanceMetrics(results.Select(r => r.Duration), results.Sum(r => r.FileSize), timer.Elapsed);
        var labelsByScan = new Dictionary<Guid,bool>();
        var tp = 0; var tn = 0; var fp = 0; var fn = 0;
        foreach (var r in results)
        {
            var key = Path.GetRelativePath(Path.GetFullPath(request.DatasetDirectory), r.FilePath).Replace(Path.DirectorySeparatorChar, '/');
            if (request.ExpectedLabels is null || !request.ExpectedLabels.TryGetValue(key, out var positive)) continue;
            labelsByScan.Add(r.ScanId, positive);
            var detected = r.DetectionLevel >= DetectionLevel.Medium;
            if (positive) { if (detected) tp++; else fn++; } else { if (detected) fp++; else tn++; }
        }
        if (labelsByScan.Count > 0) await sampleLabels.SetLabelsAsync(labelsByScan, ct);
        var labels = tp + tn + fp + fn;
        var d = BenchmarkMetricCalculator.CalculateDetectionMetrics(tp, tn, fp, fn);
        var result = new BenchmarkDto {
            BenchmarkId = id, ExperimentId = request.ExperimentId, RoundId = request.RoundId, WallabeeVersion = WallabeeVersion.Current, HoneyBadgerVersion = request.HoneyBadgerVersion,
            SamplesScanned = results.Count, SamplesDetected = results.Count(r => r.DetectionLevel >= DetectionLevel.Medium), SamplesMissed = fn, LabeledSamples = labels,
            TruePositives = tp, TrueNegatives = tn, FalsePositives = fp, FalseNegatives = fn,
            DetectionRate = tp + fn == 0 ? null : d.DetectionRate, EvasionRate = tp + fn == 0 ? null : d.FalseNegativeRate,
            Precision = tp + fp == 0 ? null : d.Precision, Recall = tp + fn == 0 ? null : d.Recall, F1Score = labels == 0 ? null : d.F1Score,
            FalsePositiveRate = tn + fp == 0 ? null : d.FalsePositiveRate, FalseNegativeRate = tp + fn == 0 ? null : d.FalseNegativeRate,
            TotalScanTimeMs = timer.Elapsed.TotalMilliseconds, AverageScanMs = performance.MeanLatency.TotalMilliseconds,
            P50ScanMs = performance.MedianLatency.TotalMilliseconds, P95ScanMs = performance.P95Latency.TotalMilliseconds, P99ScanMs = performance.P99Latency.TotalMilliseconds,
            FilesPerSecond = performance.FilesPerSecond, BytesPerSecond = performance.BytesPerSecond, MbPerSecond = performance.MegabytesPerSecond,
            CpuAverage = system?.CpuAverage, CpuPeak = system?.CpuPeak, RamAverageMb = system?.RamAverageMb, RamPeakMb = system?.RamPeakMb,
            StartedAt = started, CompletedAt = DateTimeOffset.UtcNow };
        var json = JsonSerializer.Serialize(result, ScanMapping.Json);
        var events = new List<TelemetryEvent> { new(id, "benchmark", json) };
        if (request.ExperimentId is not null && request.RoundId is not null)
            events.Add(new(Guid.NewGuid(), "round", JsonSerializer.Serialize(JsonSerializer.Deserialize<RoundSummary>(json, ScanMapping.Json)! with { SamplesGenerated = request.SamplesGenerated ?? results.Count }, ScanMapping.Json)));
        if (system is not null) events.Add(new(Guid.NewGuid(), "system", JsonSerializer.Serialize(system, ScanMapping.Json)));
        activity?.Stage("Guardando benchmark");
        await repository.SaveAsync(id, "benchmark", json, events, ct);
        logger.LogInformation("BenchmarkCompleted {BenchmarkId} {SamplesScanned}", id, results.Count);
        activity?.Complete(id);
        return result;
    }
    public async Task<BenchmarkDto?> GetAsync(Guid? id, CancellationToken ct = default) => await repository.GetAsync(id, "benchmark", ct) is {} json ? JsonSerializer.Deserialize<BenchmarkDto>(json, ScanMapping.Json) : null;
}
