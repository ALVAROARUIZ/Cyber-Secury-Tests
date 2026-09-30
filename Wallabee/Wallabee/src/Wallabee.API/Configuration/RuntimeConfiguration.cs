namespace Wallabee.API.Configuration;
public static class RuntimeConfiguration
{
    public static void ResolveLocalPaths(this WebApplicationBuilder builder)
    {
        foreach (var (key, fallback) in new[] { ("Wallabee:LabRoot", "../../lab"), ("Quarantine:Directory", "../../data/quarantine") })
        {
            var path = builder.Configuration[key] ?? fallback;
            builder.Configuration[key] = Path.GetFullPath(path, builder.Environment.ContentRootPath);
        }
    }
}
