using System.IO.Compression;
using System.Text.Json;
using Wallabee.Launcher;

namespace Wallabee.Launcher.Tests;

public sealed class BundleTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "wallabee-bundle-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void FirstLaunchExtractsApiAndHtmlAndCreatesSettings()
    {
        using var zip = Bundle();
        BundleInstaller.Install(root, zip);
        Assert.Equal("bundle-test", File.ReadAllText(Path.Combine(root, "current.txt")));
        Assert.True(File.Exists(Path.Combine(root, "versions", "bundle-test", "wwwroot", "index.html")));
        Assert.Equal("{\"port\":5212}", File.ReadAllText(Path.Combine(root, "wallabee.settings.json")));
    }

    [Fact]
    public void ReopeningSameBundlePreservesAutomaticUpdateAndUserSettings()
    {
        using (var zip = Bundle()) BundleInstaller.Install(root, zip);
        File.WriteAllText(Path.Combine(root, "current.txt"), "updated-version");
        File.WriteAllText(Path.Combine(root, "wallabee.settings.json"), "custom settings");
        using (var zip = Bundle()) BundleInstaller.Install(root, zip);
        Assert.Equal("updated-version", File.ReadAllText(Path.Combine(root, "current.txt")));
        Assert.Equal("custom settings", File.ReadAllText(Path.Combine(root, "wallabee.settings.json")));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void IncompleteOrEscapingBundleDoesNotActivate(bool omitUi, bool traversal)
    {
        using var zip = Bundle(omitUi, traversal);
        var error = Record.Exception(() => BundleInstaller.Install(root, zip));
        Assert.True(error is InvalidDataException or IOException);
        Assert.False(File.Exists(Path.Combine(root, "current.txt")));
        Assert.False(File.Exists(Path.Combine(root, "escape.txt")));
    }

    private static MemoryStream Bundle(bool omitUi = false, bool traversal = false)
    {
        var buffer = new MemoryStream();
        using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, true))
        {
            void Add(string name, string text)
            {
                using var writer = new StreamWriter(zip.CreateEntry(name).Open());
                writer.Write(text);
            }
            Add("version.json", JsonSerializer.Serialize(new { build = "bundle-test", packageFormat = 2 }));
            Add("Wallabee.exe", "supervisor");
            Add("Wallabee.API.exe", "api");
            Add("default.settings.json", "{\"port\":5212}");
            if (!omitUi) foreach (var file in new[] { "index.html", "dashboard.css", "dashboard.js" }) Add("wwwroot/" + file, "ui");
            if (traversal) Add("../../escape.txt", "not allowed");
        }
        buffer.Position = 0;
        return buffer;
    }

    public void Dispose() { if (Directory.Exists(root)) Directory.Delete(root, true); }
}
