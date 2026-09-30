using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Wallabee.API.Monitoring;
namespace Wallabee.API.Controllers;
[ApiController, Route("api/dashboard")]
public sealed class DashboardController(DashboardState state) : ControllerBase
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    [HttpGet]
    public IActionResult Snapshot() => Ok(state.Snapshot());
    [HttpGet("events")]
    public async Task Events(CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache, no-store";
        Response.Headers["X-Accel-Buffering"] = "no";
        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Response.WriteAsync("event: snapshot\ndata: " + JsonSerializer.Serialize(state.Snapshot(), Json) + "\n\n", ct);
                await Response.Body.FlushAsync(ct);
                await Task.Delay(1000, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
    }
}
