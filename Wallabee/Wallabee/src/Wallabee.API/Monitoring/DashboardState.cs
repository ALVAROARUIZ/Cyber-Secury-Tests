using System.Diagnostics;
using Wallabee.Application.Scanning.Services;
using Wallabee.Application.Telemetry;
using Wallabee.Application.Monitoring;
using Wallabee.Benchmark.Runner;
using Wallabee.Contracts.Responses;
using Wallabee.Contracts.Benchmark;
using Wallabee.Core.Interfaces;

namespace Wallabee.API.Monitoring;
public sealed record ResourcePoint(DateTimeOffset Timestamp, double Cpu, double MemoryMb);
public sealed record PersistedOverview(IReadOnlyList<ScanResponse> Scans, BenchmarkDto? Benchmark, TelemetryStatus? Telemetry,
    DateTimeOffset? UpdatedAt, bool Available, string? Error);
public sealed class DashboardState(IServiceScopeFactory scopes, ActivityMonitor activity, ILogger<DashboardState> logger) : BackgroundService
{
    private PersistedOverview overview = new([], null, null, null, false, null);
    private ResourcePoint[] resources = [];
    public object Snapshot() => new { Timestamp = DateTimeOffset.UtcNow, Overview = Volatile.Read(ref overview), Resources = Volatile.Read(ref resources), Activities = activity.Snapshot() };
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.WhenAll(SampleResourcesAsync(stoppingToken), RefreshResultsAsync(stoppingToken));
    private async Task SampleResourcesAsync(CancellationToken ct)
    {
        using var process = Process.GetCurrentProcess();
        var previous = process.TotalProcessorTime; var watch = Stopwatch.StartNew();
        try
        {
            while (!ct.IsCancellationRequested)
            {
                process.Refresh(); var cpu = process.TotalProcessorTime;
                var percent = watch.Elapsed.TotalMilliseconds < 1 ? 0 : Math.Clamp((cpu - previous).TotalMilliseconds / watch.Elapsed.TotalMilliseconds / Environment.ProcessorCount * 100, 0, 100);
                previous = cpu; watch.Restart();
                var point = new ResourcePoint(DateTimeOffset.UtcNow, percent, process.WorkingSet64 / 1048576d);
                Volatile.Write(ref resources, resources.TakeLast(119).Append(point).ToArray());
                await Task.Delay(1000, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
    }
    private async Task RefreshResultsAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await using var scope = scopes.CreateAsyncScope();
                    var scans = await scope.ServiceProvider.GetRequiredService<ScanQueryService>().FindAsync(new(Limit: 200), ct);
                    var benchmark = await scope.ServiceProvider.GetRequiredService<BenchmarkRunner>().GetAsync(null, ct);
                    var telemetry = await scope.ServiceProvider.GetRequiredService<TelemetryService>().GetStatusAsync(ct);
                    Volatile.Write(ref overview, new(scans, benchmark, telemetry, DateTimeOffset.UtcNow, true, null));
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
                catch (Exception ex)
                {
                    Volatile.Write(ref overview, overview with { Available = false, Error = "No se pudo actualizar la persistencia. Se muestran los últimos datos recibidos." });
                    logger.LogWarning("Dashboard persistence refresh failed {ErrorType}", ex.GetType().Name);
                }
                await Task.Delay(3000, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
    }
}
