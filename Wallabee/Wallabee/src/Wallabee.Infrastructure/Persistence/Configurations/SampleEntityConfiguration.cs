using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class SampleEntityConfiguration : IEntityTypeConfiguration<SampleEntity>
{
    public void Configure(EntityTypeBuilder<SampleEntity> b)
    {
        b.ToTable("Samples", "lab");
        b.HasKey(e => e.SampleId);
        b.Property(e => e.SampleId).HasColumnType("uniqueidentifier").IsRequired().ValueGeneratedNever();
        b.Property(e => e.ExternalSampleId).HasColumnType("nvarchar(200)").HasMaxLength(200);
        b.Property(e => e.ExperimentId).HasColumnType("uniqueidentifier");
        b.Property(e => e.RoundId).HasColumnType("uniqueidentifier");
        b.Property(e => e.ParentSampleId).HasColumnType("uniqueidentifier");
        b.Property(e => e.SampleFamily).HasColumnType("nvarchar(100)").HasMaxLength(100);
        b.Property(e => e.SampleType).HasColumnType("nvarchar(100)").HasMaxLength(100);
        b.Property(e => e.GroundTruthLabel).HasColumnType("varchar(30)").HasMaxLength(30).IsUnicode(false);
        b.Property(e => e.GenerationNumber).HasColumnType("int");
        b.Property(e => e.FileName).HasColumnType("nvarchar(260)").HasMaxLength(260);
        b.Property(e => e.RelativePath).HasColumnType("nvarchar(500)").HasMaxLength(500);
        b.Property(e => e.Sha256).HasColumnType("char(64)").HasMaxLength(64).IsUnicode(false).IsFixedLength().IsRequired();
        b.Property(e => e.SizeBytes).HasColumnType("bigint").IsRequired();
        b.Property(e => e.Entropy).HasColumnType("decimal(9,6)").HasPrecision(9, 6);
        b.Property(e => e.MutationStrategy).HasColumnType("nvarchar(150)").HasMaxLength(150);
        b.Property(e => e.MutationParametersJson).HasColumnType("nvarchar(max)");
        b.Property(e => e.FitnessScore).HasColumnType("decimal(9,6)").HasPrecision(9, 6);
        b.Property(e => e.RetainSample).HasColumnType("bit").IsRequired();
        b.Property(e => e.RetainReason).HasColumnType("varchar(50)").HasMaxLength(50).IsUnicode(false);
        b.Property(e => e.ExternalStorageReference).HasColumnType("nvarchar(500)").HasMaxLength(500);
        b.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        b.HasOne<ExperimentEntity>().WithMany().HasForeignKey(e => e.ExperimentId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Samples_Experiment");
        b.HasOne<ExperimentRoundEntity>().WithMany().HasForeignKey(e => e.RoundId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Samples_Round");
        b.HasOne<SampleEntity>().WithMany().HasForeignKey(e => e.ParentSampleId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Samples_ParentSample");
        b.HasIndex(e => e.Sha256).HasDatabaseName("IX_Samples_Sha256");
        b.HasIndex(e => new { e.ExperimentId, e.RoundId }).HasDatabaseName("IX_Samples_Experiment_Round");
        b.HasIndex(e => new { e.GenerationNumber, e.SampleFamily }).HasDatabaseName("IX_Samples_Generation_Family");
        b.HasIndex(e => e.RetainSample).HasDatabaseName("IX_Samples_Retained");
        b.HasIndex(e => new { e.ExperimentId, e.RoundId, e.ExternalSampleId }).IsUnique().HasFilter("[ExternalSampleId] IS NOT NULL");
    }
}
