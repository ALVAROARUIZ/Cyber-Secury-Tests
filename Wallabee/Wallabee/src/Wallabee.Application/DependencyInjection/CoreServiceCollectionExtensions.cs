using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wallabee.Core.Configuration;
using Wallabee.Core.Heuristics;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Scoring;
namespace Wallabee.Application.DependencyInjection;
// Composition belongs outside Core so the domain stays free of framework dependencies.
public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddWallabeeCore(this IServiceCollection services, IConfiguration configuration)
    {
        var scoring = configuration.GetSection("Scoring").Get<ScoringOptions>() ?? new();
        if (!(0 < scoring.Low && scoring.Low < scoring.Medium && scoring.Medium < scoring.High && scoring.High < scoring.Critical && scoring.Critical <= 100)) throw new InvalidOperationException("Invalid scoring thresholds.");
        services.AddSingleton<IRiskScorer>(new IndicatorRiskScorer(scoring));
        services.AddSingleton<IPolicyEngine>(new Wallabee.Core.Policies.PolicyEngine(configuration.GetValue<bool>("Quarantine:AutoQuarantine")));
        services.AddSingleton(configuration.GetSection("Heuristics").Get<HeuristicOptions>() ?? new());
        services.AddSingleton<IHeuristicRule, HighEntropyRule>();
        services.AddSingleton<IHeuristicRule, ExtensionMismatchRule>();
        services.AddSingleton<IHeuristicRule, UnusualFileSizeRule>();
        services.AddSingleton<IHeuristicRule, KnownHashRule>();
        services.AddSingleton<IHeuristicRule, MetadataMismatchRule>();
        services.AddSingleton<IHeuristicRule, RepeatedPatternsRule>();
        services.AddSingleton<IHeuristicRule, SyntheticAnomalyRule>();
        services.AddSingleton<IHeuristicAnalyzer, HeuristicEngine>();
        return services;
    }
}
