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

        logger.LogInformation("ScanStarted {FilePath} {FileSize}", fullPath, fileInfo.Length);
        await using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, options.Value.ScanBufferSize, FileOptions.Asynchronous);
        var hash = await hashCalculator.CalculateAsync(stream, cancellationToken).ConfigureAwait(false);
        var context = new FileContext(new FileMetadata(fullPath, fileInfo.Name, fileInfo.Length, fileInfo.LastWriteTimeUtc), hash);
        var assessment = await detectionPipeline.AnalyzeAsync(context, cancellationToken).ConfigureAwait(false);
        var result = new FileScanResult(fullPath, fileInfo.Length, hash.Value, assessment.Score, assessment.Level, assessment.Indicators, DateTimeOffset.UtcNow - startedAt);
        logger.LogInformation("ScanCompleted {FilePath} {DetectionLevel} {RiskScore}", fullPath, result.DetectionLevel, result.RiskScore);
        return result;
    }

    public async Task<IReadOnlyCollection<FileScanResult>> ScanDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        var fullPath = labPathValidator.EnsureInsideLabDirectory(directoryPath);
        var files = Directory.EnumerateFiles(fullPath, "*", SearchOption.AllDirectories);
        var results = new List<FileScanResult>();
        using var semaphore = new SemaphoreSlim(Math.Max(1, options.Value.MaximumParallelScans));
        var tasks = files.Select(async file =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await ScanFileAsync(file, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        });

        results.AddRange(await Task.WhenAll(tasks).ConfigureAwait(false));
        return results;
    }
}