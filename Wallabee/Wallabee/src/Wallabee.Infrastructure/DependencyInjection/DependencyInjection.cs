using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wallabee.Core.Configuration;
using Wallabee.Core.Heuristics;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Scoring;
using Wallabee.Application.DependencyInjection;
using Wallabee.Infrastructure.FileSystem;
using Wallabee.Infrastructure.Hashing;
using Wallabee.Infrastructure.Quarantine;

namespace Wallabee.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWallabeeInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<WallabeeOptions>().Bind(configuration.GetSection(WallabeeOptions.SectionName))
            .Validate(o => o.MaximumFileSize > 0 && o.MaximumParallelScans is > 0 and <= 64 && o.ScanBufferSize is >= 4096 and <= 1048576, "Invalid scanner limits.").ValidateOnStart();
        services.Configure<ScannerOptions>(configuration.GetSection(ScannerOptions.SectionName));
        services.Configure<QuarantineOptions>(configuration.GetSection(QuarantineOptions.SectionName));
        services.AddSingleton(provider =>
        {
            var labRoot = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<WallabeeOptions>>().Value.LabRoot;
            return new LabPathValidator(labRoot);
        });
        services.AddSingleton<IHashCalculator, Sha256HashCalculator>();
        services.AddWallabeeCore(configuration);
        services.AddWallabeePersistence(configuration);
        services.AddScoped<IFileScanner, LocalFileSystemScanner>();
        services.AddSingleton<IFileCatalog, LocalFileCatalog>();
        services.AddSingleton<IQuarantineService, LocalQuarantineService>();
        return services;
    }
}