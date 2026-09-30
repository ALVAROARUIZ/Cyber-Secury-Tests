using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Wallabee.API.Controllers;
namespace Wallabee.API.Tests;
public sealed class HealthControllerTests
{
    [Theory] [InlineData(HealthStatus.Healthy,200)] [InlineData(HealthStatus.Unhealthy,503)]
    public async Task IncludesSqlServerStatus(HealthStatus status,int code)
    {
        var response=Assert.IsType<ObjectResult>(await new HealthController(new FakeHealth(status)).Get());
        Assert.Equal(code,response.StatusCode);
    }
    private sealed class FakeHealth(HealthStatus status):HealthCheckService
    {
        public override Task<HealthReport> CheckHealthAsync(Func<HealthCheckRegistration,bool>? predicate,CancellationToken cancellationToken=default) =>
            Task.FromResult(new HealthReport(new Dictionary<string,HealthReportEntry>{{"sqlserver",new(status,"SQL Server and persistence",TimeSpan.Zero,null,null)}},TimeSpan.Zero));
    }
}
