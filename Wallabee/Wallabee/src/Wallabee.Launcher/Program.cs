using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Wallabee.Launcher;

internal static partial class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private static readonly CancellationTokenSource Shutdown = new();
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(10) };
    private static Process? api;

    private static async Task<int> Main(string[] args)
    {
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; Shutdown.Cancel(); };
        try
        {
            if (args.Length == 2 && args[0] == "--supervisor")
                return await SuperviseAsync(Path.GetFullPath(args[1]));

            var root = AppContext.BaseDirectory;
            // The stable entry point delegates to the currently installed updater version.
            using var installationLock = new FileStream(Path.Combine(root, ".wallabee.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            using var lifetime = new ProcessLifetime();
            while (!Shutdown.IsCancellationRequested)
            {
                var version = ReadVersion(root);
                var start = new ProcessStartInfo(Path.Combine(VersionDirectory(root, version), "Wallabee.exe")) { UseShellExecute = false };
                start.ArgumentList.Add("--supervisor");
                start.ArgumentList.Add(root);
                using var supervisor = Process.Start(start) ?? throw new InvalidOperationException("Cannot start Wallabee supervisor.");
                lifetime.Add(supervisor);
                await supervisor.WaitForExitAsync();
                if (supervisor.ExitCode != 42) return supervisor.ExitCode;
            }
            return 0;
        }
        catch (OperationCanceledException) { return 0; }
        catch (Exception ex) { Console.Error.WriteLine($"Wallabee: {ex.Message}"); return 1; }
        finally { StopApi(); }
    }

    private static async Task<int> SuperviseAsync(string root)
    {
        var settings = JsonSerializer.Deserialize<Settings>(await File.ReadAllTextAsync(Path.Combine(root, "wallabee.settings.json")), JsonOptions) ?? new();
        if (settings.Port is < 1024 or > 65535 || settings.CheckIntervalSeconds < 10)
            throw new InvalidOperationException("Invalid port or update interval in wallabee.settings.json.");
        if (!RepositoryPattern().IsMatch(settings.Repository)) throw new InvalidOperationException("Invalid GitHub repository.");
        var current = ReadVersion(root);
        Directory.CreateDirectory(Path.Combine(root, "lab"));
        Directory.CreateDirectory(Path.Combine(root, "logs"));
        api = StartApi(root, current, settings);
        if (!await IsHealthyAsync(api, settings.Port, current)) throw new InvalidOperationException("The API did not start. Check logs/api.log and whether the port is occupied.");
        Log(root, $"Running {current} at http://localhost:{settings.Port}. Updates every {settings.CheckIntervalSeconds} seconds. Ctrl+C to stop.");
        while (!Shutdown.IsCancellationRequested)
        {
            if (api.HasExited) throw new InvalidOperationException($"API exited with code {api.ExitCode}. See logs/api.log.");
            if (settings.AutoUpdate)
            {
                try
                {
                    var candidate = await DownloadUpdateAsync(root, current, settings, Shutdown.Token);
                    if (candidate is not null)
                    {
                        Log(root, $"Applying {candidate}; the API will restart.");
                        if (await ActivateUpdateAsync(root, current, candidate, async version =>
                        {
                            StopApi();
                            api = StartApi(root, version, settings);
                            return await IsHealthyAsync(api, settings.Port, version);
                        }))
                        {
                            StopApi();
                            Log(root, $"Updated to {candidate}.");
                            return 42;
                        }
                        Log(root, $"Version {candidate} failed its health check. Restoring {current}.");
                    }
                }
                catch (OperationCanceledException) when (Shutdown.IsCancellationRequested) { break; }
                catch (Exception ex)
                {
                    Log(root, $"Update deferred: {ex.Message}");
                    if (api is null || api.HasExited)
                    {
                        StopApi();
                        api = StartApi(root, current, settings);
                        if (!await IsHealthyAsync(api, settings.Port, current)) throw;
                    }
                }
            }
            using var interval = CancellationTokenSource.CreateLinkedTokenSource(Shutdown.Token);
            var delay = Task.Delay(TimeSpan.FromSeconds(settings.CheckIntervalSeconds), interval.Token);
            await Task.WhenAny(delay, api!.WaitForExitAsync(interval.Token));
            await interval.CancelAsync();
            Shutdown.Token.ThrowIfCancellationRequested();
        }
        return 0;
    }

    internal static async Task<bool> ActivateUpdateAsync(string root, string current, string candidate, Func<string, Task<bool>> startAndCheck)
    {
        ValidateVersion(candidate);
        bool healthy;
        try { healthy = await startAndCheck(candidate); }
        catch (OperationCanceledException) { throw; }
        catch { healthy = false; }
        if (!healthy)
        {
            await File.WriteAllTextAsync(Path.Combine(root, "failed-version.txt"), candidate);
            if (!await startAndCheck(current)) throw new InvalidOperationException("Rollback failed.");
            return false;
        }
        await File.WriteAllTextAsync(Path.Combine(root, "current.txt.new"), candidate);
        File.Move(Path.Combine(root, "current.txt.new"), Path.Combine(root, "current.txt"), true);
        return true;
    }

    private static Process StartApi(string root, string version, Settings settings)
    {
        var directory = VersionDirectory(root, version);
        var start = new ProcessStartInfo(Path.Combine(directory, "Wallabee.API.exe"))
        {
            WorkingDirectory = directory, UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        start.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{settings.Port}";
        start.Environment["Wallabee__LabRoot"] = Path.Combine(root, "lab");
        start.Environment["Quarantine__Directory"] = Path.Combine(root, "lab", "quarantine");
        start.Environment["Quarantine__AllowRestore"] = settings.AllowRestore.ToString();
        start.Environment["WallabeeRuntime__UseHttpsRedirection"] = "false";
        // Do not pass the private repository credential to the scanning API.
        start.Environment.Remove("WALLABEE_GITHUB_TOKEN");
        var process = new Process { StartInfo = start };
        void Capture(object _, DataReceivedEventArgs e)
        {
            if (e.Data is null) return;
            lock (JsonOptions) File.AppendAllText(Path.Combine(root, "logs", "api.log"), e.Data + Environment.NewLine);
        }
        process.OutputDataReceived += Capture;
        process.ErrorDataReceived += Capture;
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    private static void StopApi()
    {
        if (api is null) return;
        try { if (!api.HasExited) { api.Kill(entireProcessTree: true); api.WaitForExit(); } }
        finally { api.Dispose(); api = null; }
    }

    private static async Task<bool> IsHealthyAsync(Process process, int port, string version)
    {
        using var probe = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        for (var attempt = 0; attempt < 30; attempt++)
        {
            if (process.HasExited || Shutdown.IsCancellationRequested) return false;
            try
            {
                var health = await probe.GetFromJsonAsync<JsonElement>($"http://127.0.0.1:{port}/api/health", Shutdown.Token);
                if (health.GetProperty("build").GetString() == version && !process.HasExited) return true;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException) { }
            await Task.Delay(1000, Shutdown.Token);
        }
        return false;
    }

    internal static async Task<string?> DownloadUpdateAsync(string root, string current, Settings settings, CancellationToken token, HttpClient? client = null)
    {
        var endpoint = $"https://api.github.com/repos/{settings.Repository}";
        using var releaseResponse = await GetGitHubAsync(endpoint + "/releases/latest", false, token, client);
        releaseResponse.EnsureSuccessStatusCode();
        var release = await releaseResponse.Content.ReadFromJsonAsync<JsonElement>(token);
        var tag = release.GetProperty("tag_name").GetString() ?? "";
        if (!tag.StartsWith("wallabee-", StringComparison.Ordinal)) return null;
        var version = tag[9..];
        ValidateVersion(version);
        if (version == current) return null;
        var failed = Path.Combine(root, "failed-version.txt");
        if (File.Exists(failed) && (await File.ReadAllTextAsync(failed, token)).Trim() == version) return null;
        var assets = release.GetProperty("assets").EnumerateArray().ToArray();
        var archive = assets.Single(a => a.GetProperty("name").GetString() == "Wallabee-update-win-x64.zip");
        var checksum = assets.Single(a => a.GetProperty("name").GetString() == "Wallabee-update-win-x64.zip.sha256");
        string AssetUrl(JsonElement asset) => endpoint + "/releases/assets/" + asset.GetProperty("id").GetInt64();
        using var hashResponse = await GetGitHubAsync(AssetUrl(checksum), true, token, client);
        hashResponse.EnsureSuccessStatusCode();
        var expected = (await hashResponse.Content.ReadAsStringAsync(token)).Trim().Split(' ', '\t', '\r', '\n')[0];
        if (!HashPattern().IsMatch(expected)) throw new InvalidDataException("Invalid package checksum.");
        var staging = Path.Combine(root, "downloads", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);
        try
        {
            var zipPath = Path.Combine(staging, "update.zip");
            using (var response = await GetGitHubAsync(AssetUrl(archive), true, token, client))
            {
                response.EnsureSuccessStatusCode();
                await using var destination = File.Create(zipPath);
                await response.Content.CopyToAsync(destination, token);
            }
            await using (var content = File.OpenRead(zipPath))
            {
                var actual = Convert.ToHexString(await SHA256.HashDataAsync(content, token));
                if (!actual.Equals(expected, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Package SHA-256 mismatch.");
            }
            var unpacked = Path.Combine(staging, "unpacked");
            ZipFile.ExtractToDirectory(zipPath, unpacked);
            var manifest = JsonSerializer.Deserialize<Manifest>(await File.ReadAllTextAsync(Path.Combine(unpacked, "version.json"), token), JsonOptions);
            if (manifest?.Build != version || !File.Exists(Path.Combine(unpacked, "Wallabee.exe")) || !File.Exists(Path.Combine(unpacked, "Wallabee.API.exe")))
                throw new InvalidDataException("Package contents do not match the release.");
            var target = VersionDirectory(root, version);
            Directory.CreateDirectory(Path.Combine(root, "versions"));
            if (!Directory.Exists(target)) Directory.Move(unpacked, target);
            return version;
        }
        finally { Directory.Delete(staging, true); }
    }

    private static async Task<HttpResponseMessage> GetGitHubAsync(string url, bool asset, CancellationToken token, HttpClient? client = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("Wallabee-Updater/1.0");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(asset ? "application/octet-stream" : "application/vnd.github+json"));
        var credential = Environment.GetEnvironmentVariable("WALLABEE_GITHUB_TOKEN");
        if (!string.IsNullOrWhiteSpace(credential)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credential);
        return await (client ?? Http).SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
    }

    private static string ReadVersion(string root)
    {
        var version = File.ReadAllText(Path.Combine(root, "current.txt")).Trim();
        ValidateVersion(version);
        return version;
    }
    private static string VersionDirectory(string root, string version)
    {
        ValidateVersion(version);
        return Path.Combine(root, "versions", version);
    }
    private static void ValidateVersion(string version)
    {
        if (!VersionPattern().IsMatch(version)) throw new InvalidDataException("Invalid build identifier.");
    }
    private static void Log(string root, string message)
    {
        var line = $"{DateTimeOffset.Now:u} {message}";
        Console.WriteLine(line);
        File.AppendAllText(Path.Combine(root, "logs", "updates.log"), line + Environment.NewLine);
    }

    [GeneratedRegex("^[a-zA-Z0-9-]{1,80}$")] private static partial Regex VersionPattern();
    [GeneratedRegex("^[a-fA-F0-9]{64}$")] private static partial Regex HashPattern();
    [GeneratedRegex("^[a-zA-Z0-9_.-]+/[a-zA-Z0-9_.-]+$")] private static partial Regex RepositoryPattern();
    private sealed record Manifest(string Build);
    internal sealed record Settings
    {
        public string Repository { get; init; } = "ALVAROARUIZ/Cyber-Secury-Tests";
        public int Port { get; init; } = 5212;
        public bool AutoUpdate { get; init; } = true;
        public int CheckIntervalSeconds { get; init; } = 300;
        public bool AllowRestore { get; init; } = false;
    }
}
