using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class WallabeeVersionEntity
{
    public int WallabeeVersionId { get; set; }
    public string Version { get; set; } = null!;
    public int? MajorVersion { get; set; }
    public int? MinorVersion { get; set; }
    public int? PatchVersion { get; set; }
    public string? BuildMetadata { get; set; }
    public string? CommitHash { get; set; }
    public string? BranchName { get; set; }
    public DateTime? BuildDate { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
