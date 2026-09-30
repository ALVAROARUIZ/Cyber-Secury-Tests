using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wallabee.Core.Configuration;
using Wallabee.Core.Interfaces;
using Wallabee.Infrastructure.Persistence;
using Wallabee.Infrastructure.Persistence.Repositories;
namespace Wallabee.Infrastructure.DependencyInjection;
public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddWallabeePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PersistenceOptions>(configuration.GetSection("Persistence"));
        // AddDbContextFactory also registers a scoped DiveViewDbContext. Repositories create a fresh context per operation.
        services.AddDbContextFactory<DiveViewDbContext>((provider, options) =>
        {
            var connection = provider.GetRequiredService<IConfiguration>().GetConnectionString("DiveViewDatabase")
                ?? throw new InvalidOperationException("ConnectionStrings:DiveViewDatabase is required.");
            options.UseSqlServer(connection, sql => sql.EnableRetryOnFailure(5));
        });
        services.AddSingleton<EfUnitOfWork>();
        services.AddSingleton<ISampleLabelRepository, EfSampleLabelRepository>();
        services.AddSingleton<IScanResultRepository, EfScanResultRepository>();
        services.AddSingleton<IResultDocumentRepository, EfBenchmarkResultRepository>();
        services.AddSingleton<ITelemetryQueue, EfTelemetryQueue>();
        services.AddSingleton<IQuarantineRepository, EfQuarantineRepository>();
        services.AddHealthChecks().AddCheck<SqlServerHealthCheck>("sqlserver");
        return services;
    }
}
