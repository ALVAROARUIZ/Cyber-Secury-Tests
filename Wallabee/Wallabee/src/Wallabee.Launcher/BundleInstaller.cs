using System.IO.Compression;
using System.Reflection;
using System.Text.Json;

namespace Wallabee.Launcher;

internal static class BundleInstaller
{
    internal static string DataRoot => Path.GetFullPath(
        Environment.GetEnvironmentVariable("WALLABEE_DATA_DIRECTORY") ??
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Wallabee"));

    internal static void InstallEmbedded(string root)
    {
        using var payload = Assembly.GetExecutingAssembly().GetManifestResourceStream("Wallabee.Payload.zip");
        if (payload is null)
        {
            if (!File.Exists(Path.Combine(root, "current.txt")))
                throw new InvalidOperationException("Este ejecutable no contiene el paquete de instalación. Usa Wallabee.exe del ZIP de la release.");
            return;
        }
        Install(root, payload);
    }

    internal static void Install(string root, Stream payload)
    {
        using var archive = new ZipArchive(payload, ZipArchiveMode.Read, leaveOpen: true);
        var manifest = archive.GetEntry("version.json") ?? throw new InvalidDataException("Bundle manifest missing.");
        using var manifestStream = manifest.Open();
        using var json = JsonDocument.Parse(manifestStream);
        var version = json.RootElement.GetProperty("build").GetString() ?? "";
        if (!System.Text.RegularExpressions.Regex.IsMatch(version, "^[a-zA-Z0-9-]{1,80}$"))
            throw new InvalidDataException("Invalid bundle version.");
        var bundleMarker = Path.Combine(root, "bundle-version.txt");
        var current = Path.Combine(root, "current.txt");
        // Reopening the same download must preserve a newer automatic update.
        if (File.Exists(bundleMarker) && File.ReadAllText(bundleMarker).Trim() == version && File.Exists(current)) return;
        var staging = Path.Combine(root, "staging", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);
        try
        {
            archive.ExtractToDirectory(staging);
            foreach (var required in new[] { "Wallabee.exe", "Wallabee.API.exe", "wwwroot/index.html", "wwwroot/dashboard.js", "wwwroot/dashboard.css" })
                if (!File.Exists(Path.Combine(staging, required))) throw new InvalidDataException($"Bundle missing {required}.");
            var target = Path.Combine(root, "versions", version);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            if (!Directory.Exists(target)) Directory.Move(staging, target);
            var settings = Path.Combine(root, "wallabee.settings.json");
            if (!File.Exists(settings))
            {
                var defaults = File.ReadAllText(Path.Combine(target, "default.settings.json"));
                File.WriteAllText(settings, defaults);
            }
            File.WriteAllText(current + ".new", version);
            File.Move(current + ".new", current, true);
            File.WriteAllText(bundleMarker, version);
        }
        finally { if (Directory.Exists(staging)) Directory.Delete(staging, true); }
    }
}
