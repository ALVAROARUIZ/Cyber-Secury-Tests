using Wallabee.Core.Models;

namespace Wallabee.Core.Heuristics;

public interface IHeuristicRule
{
    string Name { get; }
    Task<ThreatIndicator?> EvaluateAsync(FileContext context, CancellationToken cancellationToken = default);
}