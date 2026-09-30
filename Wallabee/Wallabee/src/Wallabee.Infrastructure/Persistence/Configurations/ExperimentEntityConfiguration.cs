using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class ExperimentEntityConfiguration : IEntityTypeConfiguration<ExperimentEntity>
{
    public void Configure(EntityTypeBuilder<ExperimentEntity> b)
    {
        b.ToTable("Experiments", "lab");
        b.HasKey(e => e.ExperimentId);
        b.Property(e => e.ExperimentId).HasColumnType("uniqueidentifier").IsRequired().ValueGeneratedNever();
        b.Property(e => e.ExternalExperimentId).HasColumnType("nvarchar(200)").HasMaxLength(200);
        b.Property(e => e.ExperimentName).HasColumnType("nvarchar(200)").HasMaxLength(200).IsRequired();
        b.Property(e => e.Description).HasColumnType("nvarchar(1000)").HasMaxLength(1000);
        b.Property(e => e.WallabeeVersionId).HasColumnType("int").IsRequired();
        b.Property(e => e.HoneyBadgerVersionId).HasColumnType("int");
        b.Property(e => e.DatasetId).HasColumnType("uniqueidentifier");
        b.Property(e => e.TestHostId).HasColumnType("int");
        b.Property(e => e.ExperimentType).HasColumnType("varchar(50)").HasMaxLength(50).IsUnicode(false);
        b.Property(e => e.Status).HasColumnType("varchar(30)").HasMaxLength(30).IsUnicode(false).IsRequired();
        b.Property(e => e.StartedAt).HasColumnType("datetime2(3)").IsRequired();
        b.Property(e => e.CompletedAt).HasColumnType("datetime2(3)");
        b.Property(e => e.RandomSeed).HasColumnType("bigint");
        b.Property(e => e.TotalRounds).HasColumnType("int");
        b.Property(e => e.TotalSamples).HasColumnType("int");
        b.Property(e => e.ConfigurationJson).HasColumnType("nvarchar(max)");
        b.Property(e => e.IsArchived).HasColumnType("bit").IsRequired();
        b.Property(e => e.ArchivedAt).HasColumnType("datetime2(3)");
        b.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        b.HasOne<WallabeeVersionEntity>().WithMany().HasForeignKey(e => e.WallabeeVersionId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Experiments_WallabeeVersion");
        b.HasOne<HoneyBadgerVersionEntity>().WithMany().HasForeignKey(e => e.HoneyBadgerVersionId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Experiments_HoneyBadgerVersion");
        b.HasOne<DatasetEntity>().WithMany().HasForeignKey(e => e.DatasetId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Experiments_Dataset");
        b.HasOne<TestHostEntity>().WithMany().HasForeignKey(e => e.TestHostId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Experiments_TestHost");
        b.HasIndex(e => e.StartedAt).HasDatabaseName("IX_Experiments_StartedAt");
        b.HasIndex(e => new { e.WallabeeVersionId, e.Status }).HasDatabaseName("IX_Experiments_WallabeeVersion_Status");
        b.HasIndex(e => e.ExternalExperimentId).IsUnique().HasFilter("[ExternalExperimentId] IS NOT NULL");
    }
}
