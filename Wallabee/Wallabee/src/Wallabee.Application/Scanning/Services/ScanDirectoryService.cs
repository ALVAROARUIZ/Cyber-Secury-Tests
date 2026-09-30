using Microsoft.Extensions.Options;
using Wallabee.Core.Configuration;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;
namespace Wallabee.Application.Scanning.Services;
public sealed class ScanDirectoryService(IFileCatalog files, ScanFileService scans, IOptions<ScannerOptions> options, IOptions<WallabeeOptions> wallabee, Wallabee.Application.Monitoring.ActivityMonitor? monitor = null)
{
    public Task<IReadOnlyCollection<FileScanResult>> ExecuteAsync(string path, CancellationToken ct = default) => ExecuteAsync(path, new ExperimentContext(), ct);
    public async Task<IReadOnlyCollection<FileScanResult>> ExecuteAsync(string path, ExperimentContext context, CancellationToken ct = default)
    {
        if (!options.Value.AllowDirectoryScanning) throw new InvalidOperationException("Directory scanning is disabled.");
        using var activity = monitor?.Begin("directory", Path.GetFileName(Path.TrimEndingDirectorySeparator(path)), context.ExperimentId, ct);
        var paths = files.EnumerateFiles(path).ToArray();
        activity?.Total(paths.Length);
        var results = new System.Collections.Concurrent.ConcurrentBag<FileScanResult>();
        await Parallel.ForEachAsync(paths, new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, wallabee.Value.MaximumParallelScans), CancellationToken = ct },
            async (file, token) =>
            {
                results.Add(await scans.ExecuteAsync(file, context, token));
                activity?.Advance();
            });
        activity?.Complete();
        return results.ToArray();
    }
}
