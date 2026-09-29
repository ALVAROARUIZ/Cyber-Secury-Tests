using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;

namespace Wallabee.Application.Scanning.Services;

public sealed class DetectionPipeline(IHeuristicAnalyzer heuristicAnalyzer, IRiskScorer riskScorer)
{
    public async Task<RiskAssessment> AnalyzeAsync(FileContext context, CancellationToken cancellationToken = default)
    {
        var indicators = await heuristicAnalyzer.AnalyzeAsync(context, cancellationToken).ConfigureAwait(false);
        return riskScorer.Assess(indicators);
    }
}