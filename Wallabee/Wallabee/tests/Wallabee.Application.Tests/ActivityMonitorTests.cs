using Wallabee.Application.Monitoring;

namespace Wallabee.Application.Tests;

public sealed class ActivityMonitorTests
{
    [Fact]
    public void ParallelProgressAndSuccessfulResultRemainVisibleAfterDisposal()
    {
        var monitor = new ActivityMonitor();
        var result = Guid.NewGuid();
        using (var operation = monitor.Begin("directory", "dataset", "EXP"))
        {
            operation.Total(1000);
            Parallel.For(0, 1000, _ => operation.Advance());
            Assert.Equal(1000, Assert.Single(monitor.Snapshot()).Completed);
            operation.Complete(result);
        }
        var completed = Assert.Single(monitor.Snapshot());
        Assert.Equal("Completed", completed.State);
        Assert.Equal(result, completed.ResultId);
        Assert.NotNull(completed.CompletedAt);
    }

    [Theory]
    [InlineData(false, "Failed")]
    [InlineData(true, "Cancelled")]
    public void IncompleteOperationsReportFailureOrCancellation(bool cancel, string expected)
    {
        var monitor = new ActivityMonitor();
        using var cancellation = new CancellationTokenSource();
        using (monitor.Begin("file", "sample", null, cancellation.Token))
            if (cancel) cancellation.Cancel();
        Assert.Equal(expected, Assert.Single(monitor.Snapshot()).State);
    }

    [Fact]
    public void HistoryPruningPreservesActiveOperations()
    {
        var monitor = new ActivityMonitor();
        using var active = monitor.Begin("benchmark", "active", null);
        for (var i = 0; i < 250; i++)
        {
            using var operation = monitor.Begin("file", i.ToString(), null);
            operation.Complete();
        }
        using var next = monitor.Begin("file", "next", null);
        Assert.Equal(200, monitor.Snapshot().Count(x => x.State == "Completed"));
        Assert.Contains(monitor.Snapshot(), x => x.Name == "active" && x.State == "Running");
    }
}
