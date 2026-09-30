using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Wallabee.Core.Common;
namespace Wallabee.API.Controllers;
[ApiController, Route("api/health")]
public sealed class HealthController(HealthCheckService health) : ControllerBase
{
    // The launcher can open the UI even when the database is offline.
    // /api/health retains the full persistence check for diagnostics and updates.
    [HttpGet("/api/runtime")]
    public IActionResult Runtime() => Ok(new { Api = "Ready", Build = WallabeeVersion.Build });

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct = default)
    {
        var report = await health.CheckHealthAsync(ct);
        var hasSql = report.Entries.TryGetValue("sqlserver", out var sql);
        return StatusCode(report.Status == HealthStatus.Healthy ? 200 : 503, new {
            Status = report.Status.ToString(), Api = "Healthy", WallabeeVersion.Current, Build = WallabeeVersion.Build, Timestamp = DateTimeOffset.UtcNow,
            SqlServer = hasSql ? sql.Status.ToString() : "Unhealthy",
            Persistence = hasSql ? sql.Description : "Not registered" });
    }
}
