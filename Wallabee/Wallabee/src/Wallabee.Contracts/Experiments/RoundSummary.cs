using Wallabee.Contracts.Benchmark;
namespace Wallabee.Contracts.Experiments;
public sealed record RoundSummary : BenchmarkDto
{
    public int SamplesGenerated { get; init; }
}
