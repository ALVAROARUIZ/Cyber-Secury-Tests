using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;

namespace Wallabee.Application.Quarantine.Services;

public sealed class QuarantineApplicationService(IQuarantineService quarantineService)
{
    public Task<QuarantineEntry> QuarantineAsync(string path, CancellationToken cancellationToken = default) =>
        quarantineService.QuarantineAsync(path, cancellationToken);

    public Task<IReadOnlyCollection<QuarantineEntry>> ListAsync(CancellationToken cancellationToken = default) =>
        quarantineService.ListAsync(cancellationToken);

    public Task RestoreAsync(Guid entryId, CancellationToken cancellationToken = default) =>
        quarantineService.RestoreAsync(entryId, cancellationToken);

    public Task DeleteAsync(Guid entryId, CancellationToken cancellationToken = default) =>
        quarantineService.DeleteAsync(entryId, cancellationToken);
}