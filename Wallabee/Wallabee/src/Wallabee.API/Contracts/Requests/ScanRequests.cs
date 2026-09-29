using System.ComponentModel.DataAnnotations;

namespace Wallabee.API.Contracts.Requests;

public sealed class FileScanRequest
{
    [Required]
    public string FilePath { get; init; } = string.Empty;
}

public sealed class DirectoryScanRequest
{
    [Required]
    public string DirectoryPath { get; init; } = string.Empty;
}

public sealed class BenchmarkRunRequest
{
    [Required]
    public string DatasetDirectory { get; init; } = string.Empty;
}