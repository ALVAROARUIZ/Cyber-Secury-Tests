using Microsoft.Extensions.DependencyInjection;
using Wallabee.Benchmark.Runner;
using Wallabee.Benchmark.Storage;

namespace Wallabee.Benchmark.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWallabeeBenchmark(this IServiceCollection services)
    {
        services.AddScoped<BenchmarkRunner>();
        services.AddSingleton<InMemoryBenchmarkStore>();
        return services;
    }
}