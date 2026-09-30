using Microsoft.EntityFrameworkCore;
using Wallabee.Infrastructure.Persistence;
namespace Wallabee.API.Configuration;
public static class DatabaseStartup
{
    public static async Task InitializePersistenceAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment() || EF.IsDesignTime) return;
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<DiveViewDbContext>();
            await db.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
            if (!await db.Database.CanConnectAsync(app.Lifetime.ApplicationStopping)) throw new InvalidOperationException("SQL Server connection verification failed.");
            app.Logger.LogInformation("DiveViewLab persistence migrations applied and connection verified.");
        }
        catch (Exception ex)
        {
            app.Logger.LogCritical("SQL Server initialization failed {ErrorType}; check configuration and migration logs.", ex.GetType().Name);
            throw;
        }
    }
}
