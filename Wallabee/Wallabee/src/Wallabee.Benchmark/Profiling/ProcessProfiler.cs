using System.Diagnostics;
using Wallabee.Contracts.Telemetry;
namespace Wallabee.Benchmark.Profiling;
public sealed class ProcessProfiler : IAsyncDisposable
{
    private readonly Process process = Process.GetCurrentProcess();
    private readonly CancellationTokenSource stop = new();
    private readonly List<double> cpu = [];
    private readonly List<double> ram = [];
    private readonly Task sampling;
    public ProcessProfiler() { sampling = SampleAsync(); }
    private async Task SampleAsync()
    {
        var timer = Stopwatch.StartNew(); var previous = process.TotalProcessorTime;
        ram.Add(process.WorkingSet64 / 1048576d);
        try
        {
            while (true)
            {
                await Task.Delay(50, stop.Token);
                process.Refresh(); var current = process.TotalProcessorTime;
                cpu.Add(Math.Clamp((current - previous).TotalMilliseconds / timer.Elapsed.TotalMilliseconds / Environment.ProcessorCount * 100, 0, 100));
                previous = current; timer.Restart(); ram.Add(process.WorkingSet64 / 1048576d);
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        // Include the final short interval, including benchmarks shorter than 50 ms.
        process.Refresh();
        if (timer.Elapsed.TotalMilliseconds > 0) cpu.Add(Math.Clamp((process.TotalProcessorTime - previous).TotalMilliseconds / timer.Elapsed.TotalMilliseconds / Environment.ProcessorCount * 100, 0, 100));
        ram.Add(process.WorkingSet64 / 1048576d);
    }
    public async Task<SystemMetricsDto> CompleteAsync()
    {
        await stop.CancelAsync(); await sampling;
        return new(cpu.Count == 0 ? 0 : cpu.Average(), cpu.Count == 0 ? 0 : cpu.Max(), ram.Average(), ram.Max(), DateTimeOffset.UtcNow);
    }
    public async ValueTask DisposeAsync() { await stop.CancelAsync(); await sampling; stop.Dispose(); process.Dispose(); }
}
