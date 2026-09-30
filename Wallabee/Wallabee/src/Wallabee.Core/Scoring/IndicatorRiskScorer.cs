using Wallabee.Core.Enums;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;

namespace Wallabee.Core.Scoring;

public sealed class IndicatorRiskScorer(Core.Configuration.ScoringOptions? options = null) : IRiskScorer
{
    public RiskAssessment Assess(IEnumerable<ThreatIndicator> indicators)
    {
        var materializedIndicators = indicators.ToArray();
        if (materializedIndicators.Any(i => !double.IsFinite(i.Weight))) throw new ArgumentException("Indicator weights must be finite.");
        var score = Math.Clamp(materializedIndicators.Sum(indicator => indicator.Weight), 0, 100);
        return new RiskAssessment(score, ToLevel(score), materializedIndicators);
    }

    private DetectionLevel ToLevel(double score)
    {
        var thresholds = options ?? new Core.Configuration.ScoringOptions();
        return score < thresholds.Low ? DetectionLevel.Clean : score < thresholds.Medium ? DetectionLevel.Low : score < thresholds.High ? DetectionLevel.Medium : score < thresholds.Critical ? DetectionLevel.High : DetectionLevel.Critical;
    }
}