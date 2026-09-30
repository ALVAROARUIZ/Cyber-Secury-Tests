using Microsoft.AspNetCore.Mvc;
using Wallabee.Contracts.Requests;
using Wallabee.Benchmark.Runner;
namespace Wallabee.API.Controllers;
[ApiController, Route("api/benchmark")]
public sealed class BenchmarkController(BenchmarkRunner runner) : ControllerBase
{
    [HttpPost("run")]
    public async Task<IActionResult> Run(BenchmarkRunRequest request, CancellationToken ct)
    {
        var result = await runner.RunAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.BenchmarkId }, result);
    }
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest(CancellationToken ct) => await runner.GetAsync(null, ct) is {} result ? Ok(result) : NotFound();
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) => await runner.GetAsync(id, ct) is {} result ? Ok(result) : NotFound();
}
