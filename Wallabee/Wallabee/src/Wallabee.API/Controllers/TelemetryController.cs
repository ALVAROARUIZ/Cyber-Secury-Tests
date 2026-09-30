using Microsoft.AspNetCore.Mvc;
using Wallabee.Application.Telemetry;
namespace Wallabee.API.Controllers;
[ApiController, Route("api/telemetry")]
public sealed class TelemetryController(TelemetryService service) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct) => Ok(await service.GetStatusAsync(ct));
    [HttpPost("retry")]
    public async Task<IActionResult> Retry(CancellationToken ct) { await service.RetryAsync(ct); return Accepted(); }
}
