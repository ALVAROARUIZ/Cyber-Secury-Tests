using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Wallabee.Launcher;

namespace Wallabee.Launcher.Tests;

public sealed class UpdateTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "wallabee-update-tests", Guid.NewGuid().ToString("N"));

    public UpdateTests()
    {
        Directory.CreateDirectory(Path.Combine(root, "lab"));
        File.WriteAllText(Path.Combine(root, "lab", "keep.txt"), "keep my data");
        File.WriteAllText(Path.Combine(root, "current.txt"), "old-version");
    }

    [Fact]
    public async Task ValidPackage_IsStagedWithoutChangingActiveVersionOrLab()
    {
        using var client = CreateClient();
        var result = await Program.DownloadUpdateAsync(root, "old-version", new(), default, client);
        Assert.Equal("new-version", result);
        Assert.True(File.Exists(Path.Combine(root, "versions", "new-version", "Wallabee.API.exe")));
        Assert.Equal("old-version", File.ReadAllText(Path.Combine(root, "current.txt")));
        Assert.Equal("keep my data", File.ReadAllText(Path.Combine(root, "lab", "keep.txt")));
    }

    [Theory]
    [InlineData(true, "new-version", false)]
    [InlineData(false, "wrong-build", false)]
    [InlineData(false, "new-version", true)]
    public async Task InvalidPackage_IsRejectedBeforeActivation(bool corruptHash, string manifestBuild, bool traversal)
    {
        using var client = CreateClient(corruptHash, manifestBuild, traversal);
        var error = await Record.ExceptionAsync(() => Program.DownloadUpdateAsync(root, "old-version", new(), default, client));
        Assert.True(error is InvalidDataException or IOException, $"Unexpected error: {error}");
        Assert.False(Directory.Exists(Path.Combine(root, "versions", "new-version")));
        Assert.Equal("old-version", File.ReadAllText(Path.Combine(root, "current.txt")));
        Assert.Equal("keep my data", File.ReadAllText(Path.Combine(root, "lab", "keep.txt")));
        Assert.False(File.Exists(Path.Combine(root, "escape.txt")));
    }

    [Fact]
    public async Task SameVersion_DoesNotDownloadAgain()
    {
        using var client = CreateClient(corruptHash: true);
        Assert.Null(await Program.DownloadUpdateAsync(root, "new-version", new(), default, client));
        Assert.False(Directory.Exists(Path.Combine(root, "downloads")));
    }

    [Fact]
    public async Task PreviouslyFailedVersion_IsSkipped()
    {
        File.WriteAllText(Path.Combine(root, "failed-version.txt"), "new-version");
        using var client = CreateClient();
        Assert.Null(await Program.DownloadUpdateAsync(root, "old-version", new(), default, client));
    }

    [Fact]
    public async Task PrivateRepoDenied_LeavesInstallationUntouched()
    {
        using var client = new HttpClient(new Handler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)));
        await Assert.ThrowsAsync<HttpRequestException>(() => Program.DownloadUpdateAsync(root, "old-version", new(), default, client));
        Assert.Equal("old-version", File.ReadAllText(Path.Combine(root, "current.txt")));
        Assert.False(Directory.Exists(Path.Combine(root, "versions")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FailedStartup_RestartsOldVersionAndPreservesData(bool throws)
    {
        var started = new List<string>();
        var activated = await Program.ActivateUpdateAsync(root, "old-version", "new-version", version =>
        {
            started.Add(version);
            if (version == "new-version" && throws) throw new InvalidOperationException("Cannot launch executable");
            return Task.FromResult(version == "old-version");
        });
        Assert.False(activated);
        Assert.Equal(new[] { "new-version", "old-version" }, started);
        Assert.Equal("old-version", File.ReadAllText(Path.Combine(root, "current.txt")));
        Assert.Equal("new-version", File.ReadAllText(Path.Combine(root, "failed-version.txt")));
        Assert.Equal("keep my data", File.ReadAllText(Path.Combine(root, "lab", "keep.txt")));
    }

    [Fact]
    public async Task HealthyUpdate_SwitchesPointerOnlyAfterHealthCheck()
    {
        var activated = await Program.ActivateUpdateAsync(root, "old-version", "new-version", _ =>
        {
            Assert.Equal("old-version", File.ReadAllText(Path.Combine(root, "current.txt")));
            return Task.FromResult(true);
        });
        Assert.True(activated);
        Assert.Equal("new-version", File.ReadAllText(Path.Combine(root, "current.txt")));
        Assert.False(File.Exists(Path.Combine(root, "failed-version.txt")));
    }

    private static HttpClient CreateClient(bool corruptHash = false, string manifestBuild = "new-version", bool traversal = false)
    {
        using var buffer = new MemoryStream();
        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, true))
        {
            void Add(string name, string value)
            {
                using var writer = new StreamWriter(archive.CreateEntry(name).Open());
                writer.Write(value);
            }
            Add("version.json", JsonSerializer.Serialize(new { build = manifestBuild }));
            Add("Wallabee.exe", "launcher fixture");
            Add("Wallabee.API.exe", "API fixture");
            if (traversal) Add("../../../escape.txt", "must not escape");
        }
        var zip = buffer.ToArray();
        var hash = corruptHash ? new string('0', 64) : Convert.ToHexString(SHA256.HashData(zip));
        return new HttpClient(new Handler(request =>
        {
            var path = request.RequestUri!.AbsolutePath;
            HttpContent content = path.EndsWith("/latest")
                ? new StringContent(JsonSerializer.Serialize(new
                {
                    tag_name = "wallabee-new-version",
                    assets = new[] { new { id = 1, name = "Wallabee-update-win-x64.zip" }, new { id = 2, name = "Wallabee-update-win-x64.zip.sha256" } }
                }), Encoding.UTF8, "application/json")
                : path.EndsWith("/2") ? new StringContent(hash) : new ByteArrayContent(zip);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        }));
    }

    public void Dispose() => Directory.Delete(root, true);
    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(respond(request));
    }
}
