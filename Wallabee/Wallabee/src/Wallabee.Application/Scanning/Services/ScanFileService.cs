using Microsoft.Extensions.Logging;
using Wallabee.Application.Mapping;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;
using Wallabee.Core.Enums;
namespace Wallabee.Application.Scanning.Services;
public sealed class ScanFileService(IFileScanner fileScanner, IScanResultRepository repository, IPolicyEngine policy, IQuarantineService quarantine, ILogger<ScanFileService> logger, Wallabee.Application.Monitoring.ActivityMonitor? monitor = null)
{
    public Task<FileScanResult> ExecuteAsync(string path, CancellationToken cancellationToken = default) => ExecuteAsync(path, new ExperimentContext(), cancellationToken);
    public async Task<FileScanResult> ExecuteAsync(string path, ExperimentContext context, CancellationToken ct = default)
        {
        if (context.RetainSample && context.RetainReason is null) throw new ArgumentException("RetainReason is required when RetainSample is true.");
        using var activity = monitor?.Begin("file", Path.GetFileName(path), context.ExperimentId, ct);
        var analyzed = await fileScanner.ScanFileAsync(path, ct);
        activity?.Stage("Guardando resultado");
        var result = await PersistAsync(analyzed, context, ct);
        activity?.Complete(result.ScanId);
        return result;
    }
    public async Task<FileScanResult> PersistAsync(FileScanResult scan, ExperimentContext context, CancellationToken ct)
    {
        if (context.RetainSample && context.RetainReason is null) throw new ArgumentException("RetainReason is required when RetainSample is true.");
        var result = scan with { ExperimentId = context.ExperimentId, RoundId = context.RoundId, SampleId = context.SampleId,
            HoneyBadgerVersion = context.HoneyBadgerVersion, RetainSample = context.RetainSample, RetainReason = context.RetainReason,
            Indicators = scan.Indicators.Select(i => i with { ScanId = scan.ScanId }).ToArray(), ResponseAction = policy.Evaluate(scan.DetectionLevel) };
        await repository.SaveAsync(result, result.ToTelemetry(), ct);
        logger.LogInformation("TelemetryQueued {ScanId}", result.ScanId);
        if (result.ResponseAction == ResponseAction.Quarantine) await quarantine.QuarantineAsync(result.FilePath, ct);
        return result;
    }
}
