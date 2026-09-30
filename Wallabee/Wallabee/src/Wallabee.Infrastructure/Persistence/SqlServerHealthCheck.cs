using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace Wallabee.Infrastructure.Persistence;
public sealed class SqlServerHealthCheck(IDbContextFactory<DiveViewDbContext> factory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            if (!await db.Database.CanConnectAsync(cancellationToken)) return HealthCheckResult.Unhealthy("SQL Server connection unavailable.");
            if ((await db.Database.GetPendingMigrationsAsync(cancellationToken)).Any()) return HealthCheckResult.Unhealthy("Persistence migrations pending.");
            await db.Scans.AsNoTracking().Select(s => s.ScanId).Take(1).ToListAsync(cancellationToken);
            return HealthCheckResult.Healthy("SQL Server and persistence ready.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch { return HealthCheckResult.Unhealthy("SQL Server or persistence unavailable."); }
    }
}
