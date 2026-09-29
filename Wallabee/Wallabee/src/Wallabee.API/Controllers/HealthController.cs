using Microsoft.AspNetCore.Mvc;
using Wallabee.Core.Common;

namespace Wallabee.API.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { Status = "Healthy", WallabeeVersion.Current, Build = WallabeeVersion.Build, Timestamp = DateTimeOffset.UtcNow });
}
