using Microsoft.AspNetCore.Mvc;
using Wallabee.API.Contracts.Requests;
using Wallabee.Benchmark.Runner;
using Wallabee.Benchmark.Storage;

namespace Wallabee.API.Controllers;

[ApiController]
[Route("api/benchmark")]
public sealed class BenchmarkController(BenchmarkRunner benchmarkRunner, InMemoryBenchmarkStore benchmarkStore) : ControllerBase
{
    [HttpPost("run")]
    public async Task<IActionResult> Run(BenchmarkRunRequest request, CancellationToken cancellationToken)
    {
        var result = await benchmarkRunner.RunAsync(request.DatasetDirectory, cancellationToken).ConfigureAwait(false);
        benchmarkStore.Save(result);
        return AcceptedAtAction(nameof(GetById), new { id = result.Run.BenchmarkId }, result);
    }

    [HttpGet("latest")]
    public IActionResult GetLatest() => benchmarkStore.GetLatest() is { } result ? Ok(result) : NotFound();

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id) => benchmarkStore.Get(id) is { } result ? Ok(result) : NotFound();
}