using Wallabee.API.Configuration;
using Wallabee.Telemetry.DependencyInjection;
using Wallabee.Application.DependencyInjection;
using Wallabee.Benchmark.DependencyInjection;
using Wallabee.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.ResolveLocalPaths();

builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<Wallabee.API.Monitoring.DashboardState>();
builder.Services.AddHostedService(p => p.GetRequiredService<Wallabee.API.Monitoring.DashboardState>());
builder.Services.AddWallabeeApplication();
builder.Services.AddWallabeeInfrastructure(builder.Configuration);
builder.Services.AddWallabeeBenchmark(builder.Configuration);
builder.Services.AddWallabeeTelemetry(builder.Configuration);

var app = builder.Build();
await app.InitializePersistenceAsync();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (builder.Configuration.GetValue("WallabeeRuntime:UseHttpsRedirection", true))
{
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
