using Wallabee.Application.Scanning.Services;
using Wallabee.Core.Enums;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;
using Wallabee.Core.Scoring;

namespace Wallabee.Application.Tests;

public sealed class DetectionPipelineTests
{
    [Fact]
    public async Task AnalyzeAsync_UsesHeuristicIndicatorsForRisk()
    {
        var pipeline = new DetectionPipeline(new StubAnalyzer(), new IndicatorRiskScorer());
        var context = new FileContext(new FileMetadata("lab/sample.txt", "sample.txt", 1, DateTimeOffset.UtcNow));

        var result = await pipeline.AnalyzeAsync(context);

        Assert.Equal(50, result.Score);
        Assert.Equal(DetectionLevel.Medium, result.Level);
    }

    private sealed class StubAnalyzer : IHeuristicAnalyzer
    {
        public Task<IReadOnlyCollection<ThreatIndicator>> AnalyzeAsync(FileContext context, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<ThreatIndicator>>([new ThreatIndicator("test", "indicator", "test", 50)]);
    }
}