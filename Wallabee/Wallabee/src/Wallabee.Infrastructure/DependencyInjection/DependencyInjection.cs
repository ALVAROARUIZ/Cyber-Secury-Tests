using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wallabee.Core.Configuration;
using Wallabee.Core.Heuristics;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Scoring;
using Wallabee.Infrastructure.FileSystem;
using Wallabee.Infrastructure.Hashing;
using Wallabee.Infrastructure.Quarantine;

namespace Wallabee.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWallabeeInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WallabeeOptions>(configuration.GetSection(WallabeeOptions.SectionName));
        services.Configure<ScannerOptions>(configuration.GetSection(ScannerOptions.SectionName));
        services.Configure<QuarantineOptions>(configuration.GetSection(QuarantineOptions.SectionName));
        services.AddSingleton(provider =>
        {
            var labRoot = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<WallabeeOptions>>().Value.LabRoot;
            return new LabPathValidator(labRoot);
        });
        services.AddSingleton<IHashCalculator, Sha256HashCalculator>();
        services.AddSingleton<IRiskScorer, IndicatorRiskScorer>();
        services.AddSingleton<IHeuristicAnalyzer, HeuristicEngine>();
        services.AddScoped<IFileScanner, LocalFileSystemScanner>();
        services.AddSingleton<IQuarantineService, LocalQuarantineService>();
        return services;
    }
}