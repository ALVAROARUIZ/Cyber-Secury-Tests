using Microsoft.AspNetCore.Mvc;
using Wallabee.Application.Scanning.Services;
using Wallabee.Core.Interfaces;
namespace Wallabee.API.Controllers;
[ApiController, Route("api/scans")]
public sealed class ScansController(ScanQueryService service) : ControllerBase
{
    [HttpGet("{scanId:guid}")]
    public async Task<IActionResult> Get(Guid scanId, CancellationToken ct) => await service.GetAsync(scanId, ct) is {} result ? Ok(result) : NotFound();
    [HttpGet("recent")]
    public async Task<IActionResult> Recent(CancellationToken ct, int limit = 100, int offset = 0) => Ok(await service.FindAsync(new(Limit: limit, Offset: offset), ct));
    [HttpGet("hash/{sha256}")]
    public async Task<IActionResult> Hash(string sha256, CancellationToken ct, int limit = 100, int offset = 0) => Ok(await service.FindAsync(new(Sha256: sha256, Limit: limit, Offset: offset), ct));
    [HttpGet("experiment/{experimentId}")]
    [HttpGet("experiment/{experimentId}/round/{roundId}")]
    public async Task<IActionResult> Experiment(string experimentId, CancellationToken ct, string? roundId = null, string? sampleId = null, int limit = 100, int offset = 0)
        => Ok(await service.FindAsync(new(ExperimentId: experimentId, RoundId: roundId, SampleId: sampleId, Limit: limit, Offset: offset), ct));
}
