using System.Reflection;

namespace Wallabee.Core.Common;

public static class WallabeeVersion
{
    public const string Current = "0.1.0";
    public static string Build => typeof(WallabeeVersion).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .FirstOrDefault(attribute => attribute.Key == "BuildId")?.Value ?? "development";
}
