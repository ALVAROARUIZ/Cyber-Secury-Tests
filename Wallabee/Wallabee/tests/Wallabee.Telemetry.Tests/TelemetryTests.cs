using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Wallabee.Contracts.Telemetry;
using Wallabee.Core.Configuration;
using Wallabee.Core.Interfaces;


using Wallabee.Telemetry.Configuration;
using Wallabee.Telemetry.Publisher;
using Wallabee.Telemetry.Retry;
namespace Wallabee.Telemetry.Tests;
public sealed class TelemetryTests
{
    [Fact]
    public async Task PublisherRequiresMatchingAckAndStableIdempotencyKey()
    {
        var id = Guid.NewGuid(); var calls = 0;
        using var client = new HttpClient(new Handler(async request => {
            Assert.Equal(id.ToString(), Assert.Single(request.Headers.GetValues("Idempotency-Key")));
            var body = await request.Content!.ReadFromJsonAsync<TelemetryEnvelope>(); Assert.Equal(id, body!.EventId);
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(new TelemetryAck(calls++ == 0 ? Guid.NewGuid() : id, true)) };
        })) { BaseAddress = new Uri("https://example.test") };
        var publisher = new HttpTelemetryPublisher(client); var item = new TelemetryEvent(id, "scan", "{}");
        await Assert.ThrowsAsync<InvalidDataException>(() => publisher.PublishScanResultAsync(item));
        await publisher.PublishScanResultAsync(item); Assert.Equal(2, calls);
    }
    [Fact]
    public async Task DispatcherPersistsFailuresThenAcknowledgesAfterRetry()
    {
        var root = Path.Combine(Path.GetTempPath(), "wallabee-telemetry-" + Guid.NewGuid().ToString("N"));
        try
        {
            var queue = new FakeQueue();
            var item = new TelemetryEvent(Guid.NewGuid(), "scan", "{}"); await queue.EnqueueAsync(item);
            var fail = true;
            using var client = new HttpClient(new Handler(_ => Task.FromResult(new HttpResponseMessage(fail ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK) { Content = JsonContent.Create(new TelemetryAck(item.EventId, true)) }))) { BaseAddress = new Uri("https://example.test") };
            var dispatcher = new TelemetryDispatcher(queue, new HttpTelemetryPublisher(client), Options.Create(new TelemetryOptions { MaxAttempts = 1 }), NullLogger<TelemetryDispatcher>.Instance);
            await dispatcher.DispatchAsync(default); Assert.Equal(1, (await queue.GetStatusAsync()).Failed);
            fail = false; await queue.RetryAsync(); await dispatcher.DispatchAsync(default);
            Assert.Equal(1, (await queue.GetStatusAsync()).Published);
            Assert.Empty(await queue.GetDueAsync(10, DateTimeOffset.UtcNow));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
    [Fact]
    public async Task ShutdownCancellationLeavesEventPendingWithoutFailure()
    {
        var root = Path.Combine(Path.GetTempPath(), "wallabee-cancel-" + Guid.NewGuid().ToString("N"));
        try
        {
            var queue = new FakeQueue();
            await queue.EnqueueAsync(new(Guid.NewGuid(), "scan", "{}"));
            using var stop = new CancellationTokenSource();
            using var client = new HttpClient(new Handler(_ => { stop.Cancel(); throw new OperationCanceledException(stop.Token); })) { BaseAddress = new Uri("https://example.test") };
            var dispatcher = new TelemetryDispatcher(queue, new HttpTelemetryPublisher(client), Options.Create(new TelemetryOptions()), NullLogger<TelemetryDispatcher>.Instance);
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => dispatcher.DispatchAsync(stop.Token));
            Assert.Equal(new TelemetryStatus(1, 0, 0), await queue.GetStatusAsync());
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
    [Theory] [InlineData(0, 2)] [InlineData(1,5)] [InlineData(2,15)] [InlineData(3,30)] [InlineData(99,60)]
    public void BackoffIsBounded(int attempt, int seconds) => Assert.Equal(seconds, TelemetryDispatcher.RetryDelay(attempt).TotalSeconds);
    private sealed class FakeQueue : ITelemetryQueue
    {
        private readonly Dictionary<Guid,(TelemetryEvent Event,string State)> items=[];
        public Task EnqueueAsync(TelemetryEvent e,CancellationToken ct=default) { items.Add(e.EventId,(e,"Pending")); return Task.CompletedTask; }
        public Task<IReadOnlyList<TelemetryEvent>> GetDueAsync(int limit,DateTimeOffset now,CancellationToken ct=default) => Task.FromResult<IReadOnlyList<TelemetryEvent>>(items.Values.Where(x=>x.State=="Pending").Select(x=>x.Event).Take(limit).ToArray());
        public Task AcknowledgeAsync(Guid id,CancellationToken ct=default) { items[id]=(items[id].Event,"Published"); return Task.CompletedTask; }
        public Task FailAsync(Guid id,string error,DateTimeOffset next,bool exhausted,CancellationToken ct=default) { items[id]=(items[id].Event with {Attempts=items[id].Event.Attempts+1},exhausted?"Failed":"Pending"); return Task.CompletedTask; }
        public Task<TelemetryStatus> GetStatusAsync(CancellationToken ct=default) => Task.FromResult(new TelemetryStatus(items.Values.Count(x=>x.State=="Pending"),items.Values.Count(x=>x.State=="Failed"),items.Values.Count(x=>x.State=="Published")));
        public Task RetryAsync(CancellationToken ct=default) { foreach(var id in items.Keys.ToArray()) if(items[id].State!="Published") items[id]=(items[id].Event with{Attempts=0},"Pending"); return Task.CompletedTask; }
    }
    private sealed class Handler(Func<HttpRequestMessage,Task<HttpResponseMessage>> callback) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => callback(request); }
}
