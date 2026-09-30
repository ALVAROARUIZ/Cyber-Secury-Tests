using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wallabee.Infrastructure.Persistence;
namespace Wallabee.Tests.Shared;
// Real SQL Server only. Every fixture owns an isolated, uniquely named database.
public sealed class SqlServerTestDatabase : IDbContextFactory<DiveViewDbContext>, IAsyncDisposable
{
    public string ConnectionString { get; }
    public SqlServerTestDatabase()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "HoneyBadgerVsWallabee.sln"))) root = root.Parent;
        var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(root!.FullName, "src", "Wallabee.API", "appsettings.Development.json")).Build();
        var builder = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("WALLABEE_TEST_SQLSERVER") ?? config.GetConnectionString("DiveViewDatabase"));
        builder.InitialCatalog = "WallabeeTests_" + Guid.NewGuid().ToString("N");
        builder.ConnectTimeout = 10; ConnectionString = builder.ToString();
    }
    public DiveViewDbContext CreateDbContext() => new(new DbContextOptionsBuilder<DiveViewDbContext>().UseSqlServer(ConnectionString, sql => sql.EnableRetryOnFailure()).Options);
    public Task<DiveViewDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext());
    public async Task InitializeAsync()
    {
        try { await using var db = CreateDbContext(); await db.Database.MigrateAsync(); }
        catch { await DisposeAsync(); throw; }
    }
    public async ValueTask DisposeAsync()
    {
        var name = new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;
        if (!name.StartsWith("WallabeeTests_", StringComparison.Ordinal)) throw new InvalidOperationException("Refusing to delete non-test database.");
        await using var db = CreateDbContext(); await db.Database.EnsureDeletedAsync();
    }
}
