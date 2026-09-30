using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wallabee.Application.Scanning.Services;
using Wallabee.Core.Configuration;
using Wallabee.Core.Enums;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;

namespace Wallabee.Infrastructure.FileSystem;

public sealed class LocalFileSystemScanner(
    IHashCalculator hashCalculator,
    DetectionPipeline detectionPipeline,
    LabPathValidator labPathValidator,
    IOptions<WallabeeOptions> options,
    ILogger<LocalFileSystemScanner> logger) : IFileScanner
{
    public async Task<FileScanResult> ScanFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var fullPath = labPathValidator.EnsureInsideLabDirectory(filePath);
        var fileInfo = new FileInfo(fullPath);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException("The requested laboratory file was not found.", fullPath);
        }

        if (fileInfo.Length > options.Value.MaximumFileSize)
        {
            throw new InvalidOperationException("The file exceeds the configured maximum size.");
        }

        logger.LogInformation("ScanStarted {FileName} {FileSize}", fileInfo.Name, fileInfo.Length);
        await using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, options.Value.ScanBufferSize, FileOptions.Asynchronous);
        var hash = await hashCalculator.CalculateAsync(stream, cancellationToken).ConfigureAwait(false);
        var analysis = await StaticFileAnalyzer.AnalyzeAsync(stream, cancellationToken);
        var context = new FileContext(new FileMetadata(fullPath, fileInfo.Name, fileInfo.Length, fileInfo.LastWriteTimeUtc), hash, analysis);
        var assessment = await detectionPipeline.AnalyzeAsync(context, cancellationToken).ConfigureAwait(false);
        var result = new FileScanResult(fullPath, fileInfo.Length, hash.Value, assessment.Score, assessment.Level, assessment.Indicators, timer.Elapsed) { StartedAt = startedAt, CompletedAt = DateTimeOffset.UtcNow, WallabeeVersion = Wallabee.Core.Common.WallabeeVersion.Current };
        logger.LogInformation("ScanCompleted {FileName} {DetectionLevel} {RiskScore}", fileInfo.Name, result.DetectionLevel, result.RiskScore);
        if (result.DetectionLevel >= DetectionLevel.Medium) logger.LogWarning("ThreatDetected {ScanId} {DetectionLevel}", result.ScanId, result.DetectionLevel);
        return result;
    }

    public async Task<IReadOnlyCollection<FileScanResult>> ScanDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        var fullPath = labPathValidator.EnsureInsideLabDirectory(directoryPath);
        var files = Directory.EnumerateFiles(fullPath, "*", new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = FileAttributes.ReparsePoint, IgnoreInaccessible = false });
        var results = new System.Collections.Concurrent.ConcurrentBag<FileScanResult>();
        await Parallel.ForEachAsync(files, new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, options.Value.MaximumParallelScans), CancellationToken = cancellationToken },
            async (file, ct) => results.Add(await ScanFileAsync(file, ct)));
        return results.ToArray();
    }
}
