using System.Net.Http.Json;
using System.Text.Json;
using Wallabee.Contracts.Telemetry;
using Wallabee.Core.Interfaces;
namespace Wallabee.Telemetry.Publisher;
public sealed class HttpTelemetryPublisher(HttpClient client) : ITelemetryPublisher
{
    public Task PublishScanResultAsync(TelemetryEvent item, CancellationToken ct = default) => SendAsync(item, ct);
    public Task PublishBenchmarkAsync(TelemetryEvent item, CancellationToken ct = default) => SendAsync(item, ct);
    public Task PublishRoundSummaryAsync(TelemetryEvent item, CancellationToken ct = default) => SendAsync(item, ct);
    public Task PublishSystemMetricsAsync(TelemetryEvent item, CancellationToken ct = default) => SendAsync(item, ct);
    private async Task SendAsync(TelemetryEvent item, CancellationToken ct)
    {
        using var document = JsonDocument.Parse(item.Payload);
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/telemetry/events")
        { Content = JsonContent.Create(new TelemetryEnvelope(1, item.EventId, item.EventType, document.RootElement)) };
        request.Headers.Add("Idempotency-Key", item.EventId.ToString());
        using var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var ack = await response.Content.ReadFromJsonAsync<TelemetryAck>(ct);
        if (ack is null || !ack.Accepted || ack.EventId != item.EventId) throw new InvalidDataException("Telemetry ACK did not match event.");
    }
}
