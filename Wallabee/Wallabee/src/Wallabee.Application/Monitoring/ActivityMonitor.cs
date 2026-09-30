namespace Wallabee.Application.Monitoring;

public sealed record RunActivity(Guid Id, string Kind, string Name, string? ExperimentId, string State,
    string Stage, DateTimeOffset StartedAt, DateTimeOffset? CompletedAt = null, int? Total = null, int Completed = 0, Guid? ResultId = null);

// Operational state is transient. Persisted scan/benchmark results remain in the repositories.
public sealed class ActivityMonitor
{
    private readonly object gate = new();
    private readonly Dictionary<Guid, RunActivity> operations = [];
    public ActivityHandle Begin(string kind, string name, string? experimentId, CancellationToken ct = default)
    {
        var item = new RunActivity(Guid.NewGuid(), kind, name, experimentId, "Running", "Analizando", DateTimeOffset.UtcNow);
        lock (gate)
        {
            operations[item.Id] = item;
            foreach (var old in operations.Values.Where(x => x.State != "Running").OrderByDescending(x => x.StartedAt).Skip(200).ToArray()) operations.Remove(old.Id);
        }
        return new(this, item.Id, ct);
    }
    public IReadOnlyList<RunActivity> Snapshot()
    { lock (gate) return operations.Values.OrderByDescending(x => x.StartedAt).ToArray(); }
    private void Update(Guid id, Func<RunActivity, RunActivity> change)
    { lock (gate) if (operations.TryGetValue(id, out var item)) operations[id] = change(item); }
    public sealed class ActivityHandle(ActivityMonitor owner, Guid id, CancellationToken ct) : IDisposable
    {
        private bool finished;
        public void Stage(string stage) => owner.Update(id, x => x with { Stage = stage });
        public void Total(int count) => owner.Update(id, x => x with { Total = count });
        public void Advance() => owner.Update(id, x => x with { Completed = x.Completed + 1 });
        public void Complete(Guid? resultId = null)
        { finished = true; owner.Update(id, x => x with { State = "Completed", Stage = "Completado", CompletedAt = DateTimeOffset.UtcNow, ResultId = resultId }); }
        public void Dispose()
        {
            if (finished) return;
            owner.Update(id, x => x with { State = ct.IsCancellationRequested ? "Cancelled" : "Failed", Stage = ct.IsCancellationRequested ? "Cancelado" : "No se pudo completar", CompletedAt = DateTimeOffset.UtcNow });
        }
    }
}
