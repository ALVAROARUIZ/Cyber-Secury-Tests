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
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!fullPath.StartsWith(_labRoot, comparison) && !string.Equals(fullPath.TrimEnd(Path.DirectorySeparatorChar), _labRoot.TrimEnd(Path.DirectorySeparatorChar), comparison)) return false;
        // Reject symlink/junction traversal, including existing parents of restore destinations.
        for (var current = fullPath; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
            if ((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) return false;
        return true;
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