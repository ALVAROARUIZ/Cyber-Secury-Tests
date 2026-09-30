using Microsoft.AspNetCore.Mvc;
using Wallabee.Application.Quarantine.Services;

namespace Wallabee.API.Controllers;

[ApiController]
[Route("api/quarantine")]
public sealed class QuarantineController(QuarantineApplicationService quarantineService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Quarantine(Wallabee.Contracts.Requests.ScanFileRequest request, CancellationToken cancellationToken) =>
        Ok(await quarantineService.QuarantineAsync(request.FilePath, cancellationToken));

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) => Ok(await quarantineService.ListAsync(cancellationToken).ConfigureAwait(false));

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await quarantineService.RestoreAsync(id, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (Exception exception) when (exception is KeyNotFoundException or InvalidOperationException)
        {
            return NotFound(new ProblemDetails { Title = "Quarantine restore rejected", Detail = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await quarantineService.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ProblemDetails { Title = "Quarantine entry not found", Detail = exception.Message });
        }
    }
}