using Wallabee.Application.DependencyInjection;
using Wallabee.Benchmark.DependencyInjection;
using Wallabee.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var labRoot = builder.Configuration["Wallabee:LabRoot"] ?? "../../lab";
if (!Path.IsPathRooted(labRoot))
{
    builder.Configuration["Wallabee:LabRoot"] = Path.GetFullPath(labRoot, builder.Environment.ContentRootPath);
}

var quarantineDirectory = builder.Configuration["Quarantine:Directory"] ?? "../../lab/quarantine";
if (!Path.IsPathRooted(quarantineDirectory))
{
    builder.Configuration["Quarantine:Directory"] = Path.GetFullPath(quarantineDirectory, builder.Environment.ContentRootPath);
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddWallabeeApplication();
builder.Services.AddWallabeeInfrastructure(builder.Configuration);
builder.Services.AddWallabeeBenchmark();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (builder.Configuration.GetValue("WallabeeRuntime:UseHttpsRedirection", true))
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
