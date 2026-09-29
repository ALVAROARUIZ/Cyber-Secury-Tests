using Wallabee.Core.Enums;
using Wallabee.Core.Heuristics;
using Wallabee.Core.Models;
using Wallabee.Core.Scoring;

namespace Wallabee.Core.Tests;

public sealed class RiskAndHeuristicTests
{
    [Fact]
    public void Assess_ClampsScoreAndClassifiesRisk()
    {
        var result = new IndicatorRiskScorer().Assess([new ThreatIndicator("test", "signal", "test", 120)]);

        Assert.Equal(100, result.Score);
        Assert.Equal(DetectionLevel.Critical, result.Level);
    }

    [Fact]
    public async Task AnalyzeAsync_ReturnsOnlyMatchedIndicators()
    {
        var engine = new HeuristicEngine([new MatchedRule(), new NoMatchRule()]);
        var metadata = new FileMetadata("lab/sample.txt", "sample.txt", 1, DateTimeOffset.UtcNow);

        var results = await engine.AnalyzeAsync(new FileContext(metadata));

        var result = Assert.Single(results);
        Assert.Equal("matched", result.Name);
    }

    private sealed class MatchedRule : IHeuristicRule
    {
        public string Name => "matched";
        public Task<ThreatIndicator?> EvaluateAsync(FileContext context, CancellationToken cancellationToken = default) => Task.FromResult<ThreatIndicator?>(new ThreatIndicator("test", Name, "matched", 20));
    }

    private sealed class NoMatchRule : IHeuristicRule
    {
        public string Name => "no-match";
        public Task<ThreatIndicator?> EvaluateAsync(FileContext context, CancellationToken cancellationToken = default) => Task.FromResult<ThreatIndicator?>(null);
    }
}