namespace Wallabee.Infrastructure.FileSystem;

public sealed class LabPathValidator
{
    private readonly string _labRoot;

    public LabPathValidator(string labRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(labRoot);
        _labRoot = EnsureTrailingSeparator(Path.GetFullPath(labRoot));
    }

    public bool IsInsideLabDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var fullPath = Path.GetFullPath(path);
        return fullPath.StartsWith(_labRoot, StringComparison.OrdinalIgnoreCase);
    }

    public string EnsureInsideLabDirectory(string path)
    {
        if (!IsInsideLabDirectory(path))
        {
            throw new UnauthorizedAccessException("The path is outside the configured laboratory directory.");
        }

        return Path.GetFullPath(path);
    }

    private static string EnsureTrailingSeparator(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
}