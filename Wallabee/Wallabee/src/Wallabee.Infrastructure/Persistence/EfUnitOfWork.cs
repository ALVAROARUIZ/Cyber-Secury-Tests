using System.Data;
using Microsoft.EntityFrameworkCore;
namespace Wallabee.Infrastructure.Persistence;
public sealed class EfUnitOfWork(IDbContextFactory<DiveViewDbContext> factory)
{
    public async Task WriteAsync(Func<DiveViewDbContext, CancellationToken, Task> action, CancellationToken ct)
    {
        await using var strategyContext = await factory.CreateDbContextAsync(ct);
        await strategyContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            // Fresh context per attempt and per concurrent scan; never shared across threads.
            await using var db = await factory.CreateDbContextAsync(ct);
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            await action(db, ct);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });
    }
}
