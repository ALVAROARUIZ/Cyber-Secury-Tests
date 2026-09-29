using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;

namespace Wallabee.Application.Scanning.Services;

public sealed class ScanFileService(IFileScanner fileScanner)
{
    public Task<FileScanResult> ExecuteAsync(string filePath, CancellationToken cancellationToken = default) =>
        fileScanner.ScanFileAsync(filePath, cancellationToken);
}