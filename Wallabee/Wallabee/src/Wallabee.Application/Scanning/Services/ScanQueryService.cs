using Wallabee.Application.Mapping;
using Wallabee.Contracts.Responses;
using Wallabee.Core.Interfaces;
namespace Wallabee.Application.Scanning.Services;
public sealed class ScanQueryService(IScanResultRepository repository)
{
    public async Task<ScanResponse?> GetAsync(Guid id, CancellationToken ct) => (await repository.GetAsync(id, ct))?.ToResponse();
    public async Task<IReadOnlyList<ScanResponse>> FindAsync(ScanFilter filter, CancellationToken ct) => (await repository.FindAsync(filter, ct)).Select(x => x.ToResponse()).ToArray();
}
