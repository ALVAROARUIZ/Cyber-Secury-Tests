using System.Net;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Wallabee.Contracts.Requests;
using Wallabee.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Wallabee.Infrastructure.Persistence;
using Wallabee.Contracts.Responses;
using Wallabee.Contracts.Benchmark;
namespace Wallabee.API.Tests;
public sealed class ScanIntegrationTests
{
    [Fact]
    public async Task ApiScansQueriesBenchmarksAndRestoresDurableResults()
    {
        await using var database = new SqlServerTestDatabase();
        await database.InitializeAsync();
        var root = Path.Combine(Path.GetTempPath(), "wallabee-api-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        var sample = Path.Combine(root, "sample.synthetic-anomaly.bin"); await File.WriteAllTextAsync(sample, "inert sample");
        WebApplicationFactory<Program> Factory() => new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string,string?> {
            ["Wallabee:LabRoot"] = root, ["Quarantine:Directory"] = Path.Combine(root, "quarantine"),
            ["ConnectionStrings:DiveViewDatabase"] = database.ConnectionString, ["Telemetry:Enabled"] = "false",
            ["WallabeeRuntime:UseHttpsRedirection"] = "false", ["Benchmark:RecordSystemMetrics"] = "true"
        })));
        try
        {
            Guid scanId;
            using (var factory = Factory())
            using (var client = factory.CreateClient())
            {
                await using var scope = factory.Services.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<DiveViewDbContext>();
                Assert.Contains("WallabeeTests_", context.Database.GetDbConnection().Database);
                (await client.GetAsync("/api/health")).EnsureSuccessStatusCode();
                Assert.Contains("WALLABEE", await client.GetStringAsync("/"));
                (await client.GetAsync("/dashboard.js")).EnsureSuccessStatusCode();
                (await client.GetAsync("/dashboard.css")).EnsureSuccessStatusCode();
                var response = await client.PostAsJsonAsync("/api/scan/file", new ScanFileRequest { FilePath = sample, ExperimentId = "EXP", RoundId = "R1", SampleId = "S1", HoneyBadgerVersion = "0.2" });
                response.EnsureSuccessStatusCode(); var scan = (await response.Content.ReadFromJsonAsync<ScanResponse>())!; scanId = scan.ScanId;
                Assert.Equal("Medium", scan.DetectionLevel); Assert.Equal("S1", scan.SampleId); Assert.Single(scan.Indicators);
                Assert.All(scan.Indicators, i => Assert.Equal(scanId, i.ScanId));
                Assert.DoesNotContain(root, await response.Content.ReadAsStringAsync());
                var results = await client.GetFromJsonAsync<ScanResponse[]>("/api/scans/experiment/EXP/round/R1"); Assert.Single(results!);
                Assert.Empty((await client.GetFromJsonAsync<ScanResponse[]>("/api/scans/experiment/EXP/round/missing"))!);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/scan/file", new { filePath = "" })).StatusCode);
                // Dataset kept separate from DB files.
                var dataset = Path.Combine(root, "dataset"); Directory.CreateDirectory(dataset);
                await File.WriteAllTextAsync(Path.Combine(dataset, "positive.synthetic-anomaly.bin"), "inert");
                await File.WriteAllTextAsync(Path.Combine(dataset, "clean.txt"), "inert");
                var run = await client.PostAsJsonAsync("/api/benchmark/run", new BenchmarkRunRequest { DatasetDirectory = dataset, ExperimentId = "EXP", RoundId = "R2", ExpectedLabels = new() { ["positive.synthetic-anomaly.bin"] = true, ["clean.txt"] = false } });
                run.EnsureSuccessStatusCode(); var benchmark = (await run.Content.ReadFromJsonAsync<BenchmarkDto>())!;
                Assert.Equal(1, benchmark.TruePositives); Assert.Equal(1, benchmark.TrueNegatives); Assert.Equal(1d, benchmark.Recall);
                using (var dashboard = await client.GetFromJsonAsync<JsonDocument>("/api/dashboard"))
                {
                    var activities = dashboard!.RootElement.GetProperty("activities").EnumerateArray().ToArray();
                    Assert.Contains(activities, a => a.GetProperty("resultId").ToString() == scanId.ToString() && a.GetProperty("state").GetString() == "Completed");
                    Assert.Contains(activities, a => a.GetProperty("kind").GetString() == "benchmark" && a.GetProperty("state").GetString() == "Completed");
                }
                using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                using (var events = await client.GetAsync("/api/dashboard/events", HttpCompletionOption.ResponseHeadersRead, timeout.Token))
                {
                    Assert.Equal("text/event-stream", events.Content.Headers.ContentType!.MediaType);
                    using var reader = new StreamReader(await events.Content.ReadAsStreamAsync(timeout.Token));
                    DateTimeOffset? previous = null;
                    for (var i = 0; i < 2; i++)
                    {
                        Assert.Equal("event: snapshot", await reader.ReadLineAsync(timeout.Token));
                        var data = await reader.ReadLineAsync(timeout.Token);
                        Assert.StartsWith("data: ", data);
                        using var snapshot = JsonDocument.Parse(data![6..]);
                        var timestamp = snapshot.RootElement.GetProperty("timestamp").GetDateTimeOffset();
                        if (previous.HasValue) Assert.True(timestamp > previous.Value);
                        previous = timestamp;
                        Assert.NotEmpty(snapshot.RootElement.GetProperty("resources").EnumerateArray());
                        Assert.Equal("", await reader.ReadLineAsync(timeout.Token));
                    }
                    timeout.Cancel();
                }
                Assert.NotNull(benchmark.CpuAverage); Assert.True(benchmark.RamPeakMb > 0);
                Assert.Equal(benchmark.BenchmarkId, (await client.GetFromJsonAsync<BenchmarkDto>("/api/benchmark/latest"))!.BenchmarkId);
                (await client.GetAsync($"/api/benchmark/{benchmark.BenchmarkId}")).EnsureSuccessStatusCode();
                var status = await client.GetFromJsonAsync<Wallabee.Core.Interfaces.TelemetryStatus>("/api/telemetry/status"); Assert.True(status!.Pending >= 6);
            }
            using var restarted = Factory(); using var restartedClient = restarted.CreateClient();
            Assert.Equal("S1", (await restartedClient.GetFromJsonAsync<ScanResponse>($"/api/scans/{scanId}"))!.SampleId);
        }
        finally { Directory.Delete(root, true); }
    }
}
