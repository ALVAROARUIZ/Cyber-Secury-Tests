using Wallabee.Core.Interfaces;
namespace Wallabee.Infrastructure.FileSystem;
public sealed class LocalFileCatalog(LabPathValidator paths) : IFileCatalog
{
    public IEnumerable<string> EnumerateFiles(string directoryPath) => Directory.EnumerateFiles(paths.EnsureInsideLabDirectory(directoryPath), "*",
        new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = FileAttributes.ReparsePoint, IgnoreInaccessible = false });
}
