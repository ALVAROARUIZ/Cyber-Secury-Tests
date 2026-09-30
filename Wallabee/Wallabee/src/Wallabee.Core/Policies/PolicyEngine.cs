using Wallabee.Core.Enums;
using Wallabee.Core.Interfaces;
namespace Wallabee.Core.Policies;
public sealed class PolicyEngine(bool autoQuarantine = false) : IPolicyEngine
{
    public ResponseAction Evaluate(DetectionLevel level) => level switch
    {
        DetectionLevel.Clean => ResponseAction.None,
        DetectionLevel.Low => ResponseAction.TelemetryOnly,
        DetectionLevel.Critical when autoQuarantine => ResponseAction.Quarantine,
        _ => ResponseAction.Alert
    };
}
