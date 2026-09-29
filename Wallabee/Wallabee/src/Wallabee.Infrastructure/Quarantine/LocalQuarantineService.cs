using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Wallabee.Core.Configuration;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;
using Wallabee.Infrastructure.FileSystem;

namespace Wallabee.Infrastructure.Quarantine;

public sealed class LocalQuarantineService(
    LabPathValidator labPathValidator,
    IHashCalculator hashCalculator,
    IOptions<QuarantineOptions> options) : IQuarantineService
{
    private readonly ConcurrentDictionary<Guid, QuarantineEntry> _entries = new();

    public async Task<QuarantineEntry> QuarantineAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var originalPath = labPathValidator.EnsureInsideLabDirectory(filePath);
        var quarantineDirectory = labPathValidator.EnsureInsideLabDirectory(options.Value.Directory);
        Directory.CreateDirectory(quarantineDirectory);
        await using var source = File.OpenRead(originalPath);
        var hash = await hashCalculator.CalculateAsync(source, cancellationToken).ConfigureAwait(false);
        var entry = new QuarantineEntry(Guid.NewGuid(), originalPath, Path.Combine(quarantineDirectory, $"{Guid.NewGuid():N}.quarantine"), DateTimeOffset.UtcNow, hash.Value);
        File.Move(originalPath, entry.QuarantinedPath);
        _entries[entry.Id] = entry;
        return entry;
    }

    public Task RestoreAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        if (!options.Value.AllowRestore)
        {
            throw new InvalidOperationException("Quarantine restore is disabled.");
        }

        var entry = GetEntry(entryId);
        labPathValidator.EnsureInsideLabDirectory(entry.OriginalPath);
        File.Move(entry.QuarantinedPath, entry.OriginalPath);
        _entries.TryRemove(entryId, out _);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        var entry = GetEntry(entryId);
        labPathValidator.EnsureInsideLabDirectory(entry.QuarantinedPath);
        File.Delete(entry.QuarantinedPath);
        _entries.TryRemove(entryId, out _);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<QuarantineEntry>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<QuarantineEntry>>(_entries.Values.OrderBy(entry => entry.QuarantinedAt).ToArray());

    private QuarantineEntry GetEntry(Guid entryId) =>
        _entries.TryGetValue(entryId, out var entry) ? entry : throw new KeyNotFoundException("The quarantine entry was not found.");
}