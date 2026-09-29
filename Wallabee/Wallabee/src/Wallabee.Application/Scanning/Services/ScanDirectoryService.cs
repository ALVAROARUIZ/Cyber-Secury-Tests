using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;

namespace Wallabee.Application.Scanning.Services;

public sealed class ScanDirectoryService(IFileScanner fileScanner)
{
    public Task<IReadOnlyCollection<FileScanResult>> ExecuteAsync(string directoryPath, CancellationToken cancellationToken = default) =>
        fileScanner.ScanDirectoryAsync(directoryPath, cancellationToken);
}