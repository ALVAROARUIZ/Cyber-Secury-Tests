using Wallabee.Core.Enums;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;

namespace Wallabee.Core.Scoring;

public sealed class IndicatorRiskScorer : IRiskScorer
{
    public RiskAssessment Assess(IEnumerable<ThreatIndicator> indicators)
    {
        var materializedIndicators = indicators.ToArray();
        var score = Math.Clamp(materializedIndicators.Sum(indicator => indicator.Weight), 0, 100);
        return new RiskAssessment(score, ToLevel(score), materializedIndicators);
    }

    private static DetectionLevel ToLevel(int score) => score switch
    {
        0 => DetectionLevel.Clean,
        < 30 => DetectionLevel.Low,
        < 60 => DetectionLevel.Medium,
        < 85 => DetectionLevel.High,
        _ => DetectionLevel.Critical
    };
}