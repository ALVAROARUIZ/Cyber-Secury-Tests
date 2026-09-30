using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Wallabee.Core.Configuration;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;
using Wallabee.Infrastructure.FileSystem;
namespace Wallabee.Infrastructure.Quarantine;
public sealed class LocalQuarantineService(LabPathValidator paths, IHashCalculator hashes, IOptions<QuarantineOptions> options,
    IQuarantineRepository repository, ILogger<LocalQuarantineService> logger) : IQuarantineService
{
    private readonly LabPathValidator quarantinePaths = new(options.Value.Directory);
    private readonly SemaphoreSlim gate = new(1,1);
    public async Task<QuarantineEntry> QuarantineAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var original = paths.EnsureInsideLabDirectory(filePath);
            var directory = quarantinePaths.EnsureInsideLabDirectory(options.Value.Directory);
            Directory.CreateDirectory(directory);
            HashResult hash;
            await using (var source = File.OpenRead(original)) hash = await hashes.CalculateAsync(source, cancellationToken);
            var entry = new QuarantineEntry(Guid.NewGuid(), original, Path.Combine(directory, $"{Guid.NewGuid():N}.quarantine"), DateTimeOffset.UtcNow, hash.Value);
            // Persist intent first so a crash between DB and filesystem operations is recoverable.
            await repository.SaveAsync(entry, cancellationToken);
            File.Move(original, entry.QuarantinedPath);
            entry = entry with { State = "Quarantined" };
            await repository.SaveAsync(entry, CancellationToken.None);
            logger.LogInformation("FileQuarantined {QuarantineId}", entry.Id);
            return entry;
        }
        finally { gate.Release(); }
    }
    public async Task RestoreAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        if (!options.Value.AllowRestore) throw new InvalidOperationException("Quarantine restore is disabled.");
        await gate.WaitAsync(cancellationToken);
        try
        {
            var entry = await GetAsync(entryId, cancellationToken);
            paths.EnsureInsideLabDirectory(entry.OriginalPath); quarantinePaths.EnsureInsideLabDirectory(entry.QuarantinedPath);
            if (entry.State == "Restored") return;
            if (entry.State == "Deleted") throw new InvalidOperationException("Entry was deleted.");
            if (File.Exists(entry.QuarantinedPath))
            {
                await using (var source = File.OpenRead(entry.QuarantinedPath))
                    if ((await hashes.CalculateAsync(source, cancellationToken)).Value != entry.Sha256) throw new InvalidOperationException("Quarantine hash mismatch.");
                await repository.SaveAsync(entry with { State = "Restoring" }, cancellationToken);
                File.Move(entry.QuarantinedPath, entry.OriginalPath, false);
            }
            else
            {
                if (entry.State != "Restoring" || !File.Exists(entry.OriginalPath)) throw new InvalidOperationException("Quarantine file is missing.");
                await using var source = File.OpenRead(entry.OriginalPath);
                if ((await hashes.CalculateAsync(source, cancellationToken)).Value != entry.Sha256) throw new InvalidOperationException("Restored file hash mismatch.");
            }
            await repository.SaveAsync(entry with { State = "Restored", RestoredAt = DateTimeOffset.UtcNow }, CancellationToken.None);
            logger.LogInformation("FileRestored {QuarantineId}", entry.Id);
        }
        finally { gate.Release(); }
    }
    public async Task DeleteAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var entry = await GetAsync(entryId, cancellationToken);
            quarantinePaths.EnsureInsideLabDirectory(entry.QuarantinedPath);
            File.Delete(entry.QuarantinedPath);
            await repository.SaveAsync(entry with { State = "Deleted" }, CancellationToken.None);
        }
        finally { gate.Release(); }
    }
    public async Task<IReadOnlyCollection<QuarantineEntry>> ListAsync(CancellationToken cancellationToken = default) => (await repository.ListAsync(cancellationToken)).ToArray();
    private async Task<QuarantineEntry> GetAsync(Guid id, CancellationToken ct) => (await repository.ListAsync(ct)).FirstOrDefault(e => e.Id == id) ?? throw new KeyNotFoundException("Quarantine entry not found.");
}
