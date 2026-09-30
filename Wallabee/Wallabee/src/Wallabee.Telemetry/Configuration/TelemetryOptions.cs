namespace Wallabee.Telemetry.Configuration;
public sealed class TelemetryOptions
{
    public bool Enabled { get; init; } = true;
    public string BaseUrl { get; init; } = "https://localhost:7000";
    public int BatchSize { get; init; } = 100;
    public bool RetryEnabled { get; init; } = true;
    public int MaxAttempts { get; init; } = 8;
    public int PollSeconds { get; init; } = 2;
    public int TimeoutSeconds { get; init; } = 15;
}
