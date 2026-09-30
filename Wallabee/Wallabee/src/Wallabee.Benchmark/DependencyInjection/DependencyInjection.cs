using Microsoft.Extensions.DependencyInjection;
using Wallabee.Benchmark.Runner;
using Wallabee.Benchmark.Storage;

namespace Wallabee.Benchmark.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWallabeeBenchmark(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        services.AddScoped<BenchmarkRunner>();
        services.Configure<Wallabee.Core.Configuration.BenchmarkOptions>(configuration.GetSection("Benchmark"));
        return services;
    }
}