using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
namespace Wallabee.Infrastructure.Persistence;
public sealed class DiveViewDbContextFactory : IDesignTimeDbContextFactory<DiveViewDbContext>
{
    public DiveViewDbContext CreateDbContext(string[] args)
    {
        var path = Directory.GetCurrentDirectory();
        if (Directory.Exists(Path.Combine(path, "src", "Wallabee.API"))) path = Path.Combine(path, "src", "Wallabee.API");
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var config = new ConfigurationBuilder().SetBasePath(path).AddJsonFile("appsettings.json", false).AddJsonFile($"appsettings.{environment}.json", true)
            .AddUserSecrets("wallabee-diveview-development", true).AddEnvironmentVariables().Build();
        var connection = config.GetConnectionString("DiveViewDatabase") ?? throw new InvalidOperationException("ConnectionStrings:DiveViewDatabase is required.");
        return new(new DbContextOptionsBuilder<DiveViewDbContext>().UseSqlServer(connection).Options);
    }
}
