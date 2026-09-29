using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;

namespace Wallabee.Core.Heuristics;

public sealed class HeuristicEngine(IEnumerable<IHeuristicRule> rules) : IHeuristicAnalyzer
{
    public async Task<IReadOnlyCollection<ThreatIndicator>> AnalyzeAsync(
        FileContext context,
        CancellationToken cancellationToken = default)
    {
        var indicators = new List<ThreatIndicator>();
        foreach (var rule in rules)
        {
            var indicator = await rule.EvaluateAsync(context, cancellationToken).ConfigureAwait(false);
            if (indicator is not null)
            {
                indicators.Add(indicator);
            }
        }

        return indicators;
    }
}