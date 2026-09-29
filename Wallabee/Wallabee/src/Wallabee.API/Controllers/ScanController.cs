using Microsoft.AspNetCore.Mvc;
using Wallabee.API.Contracts.Requests;
using Wallabee.Application.Scanning.Services;

namespace Wallabee.API.Controllers;

[ApiController]
[Route("api/scan")]
public sealed class ScanController(ScanFileService scanFileService, ScanDirectoryService scanDirectoryService) : ControllerBase
{
    [HttpPost("file")]
    public async Task<IActionResult> ScanFile(FileScanRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await scanFileService.ExecuteAsync(request.FilePath, cancellationToken).ConfigureAwait(false));
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or FileNotFoundException or InvalidOperationException)
        {
            return BadRequest(new ProblemDetails { Title = "Scan request rejected", Detail = exception.Message });
        }
    }

    [HttpPost("directory")]
    public async Task<IActionResult> ScanDirectory(DirectoryScanRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await scanDirectoryService.ExecuteAsync(request.DirectoryPath, cancellationToken).ConfigureAwait(false));
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or DirectoryNotFoundException)
        {
            return BadRequest(new ProblemDetails { Title = "Scan request rejected", Detail = exception.Message });
        }
    }
}