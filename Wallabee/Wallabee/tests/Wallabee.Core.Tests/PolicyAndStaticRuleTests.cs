using Wallabee.Core.Enums;
using Wallabee.Core.Models;
using Wallabee.Core.Policies;
using Wallabee.Core.Scoring;
using Wallabee.Core.Configuration;
using Wallabee.Core.Heuristics;
namespace Wallabee.Core.Tests;
public sealed class PolicyAndStaticRuleTests
{
    [Theory]
    [InlineData(0, DetectionLevel.Clean)] [InlineData(19, DetectionLevel.Clean)] [InlineData(20, DetectionLevel.Low)]
    [InlineData(49, DetectionLevel.Low)] [InlineData(50, DetectionLevel.Medium)] [InlineData(74, DetectionLevel.Medium)]
    [InlineData(75, DetectionLevel.High)] [InlineData(89, DetectionLevel.High)] [InlineData(90, DetectionLevel.Critical)]
    public void Thresholds(int weight, DetectionLevel expected) => Assert.Equal(expected, new IndicatorRiskScorer().Assess([new("test", "test", "test", weight)]).Level);
    [Fact]
    public void QuarantineIsOptIn()
    {
        Assert.Equal(ResponseAction.Alert, new PolicyEngine().Evaluate(DetectionLevel.Critical));
        Assert.Equal(ResponseAction.Quarantine, new PolicyEngine(true).Evaluate(DetectionLevel.Critical));
        Assert.Equal(ResponseAction.TelemetryOnly, new PolicyEngine().Evaluate(DetectionLevel.Low));
    }
    [Fact]
    public async Task IndependentRulesEvaluateStaticMetadata()
    {
        var options = new HeuristicOptions { KnownHashes = ["ABC"], UnusualSizeBytes = 100 };
        var context = new FileContext(new("unused", "test.synthetic-anomaly.bin", 2000, DateTimeOffset.UtcNow), new("SHA256", "abc"), new(7.99, .97, ".exe", 2048));
        IHeuristicRule[] rules = [new HighEntropyRule(options), new ExtensionMismatchRule(), new KnownHashRule(options), new UnusualFileSizeRule(options), new MetadataMismatchRule(), new RepeatedPatternsRule(), new SyntheticAnomalyRule()];
        foreach (var rule in rules) Assert.NotNull(await rule.EvaluateAsync(context));
    }
}
