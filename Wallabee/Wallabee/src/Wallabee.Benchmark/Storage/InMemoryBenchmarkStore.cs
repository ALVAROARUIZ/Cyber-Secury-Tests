using System.Collections.Concurrent;
using Wallabee.Benchmark.Models;

namespace Wallabee.Benchmark.Storage;

public sealed class InMemoryBenchmarkStore
{
    private readonly ConcurrentDictionary<Guid, BenchmarkResult> _results = new();

    public void Save(BenchmarkResult result) => _results[result.Run.BenchmarkId] = result;
    public BenchmarkResult? Get(Guid benchmarkId) => _results.GetValueOrDefault(benchmarkId);
    public BenchmarkResult? GetLatest() => _results.Values.OrderByDescending(result => result.Run.Timestamp).FirstOrDefault();
}