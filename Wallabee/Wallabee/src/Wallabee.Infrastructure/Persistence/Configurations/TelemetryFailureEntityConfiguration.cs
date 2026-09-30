using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class TelemetryFailureEntityConfiguration : IEntityTypeConfiguration<TelemetryFailureEntity>
{
    public void Configure(EntityTypeBuilder<TelemetryFailureEntity> b)
    {
        b.ToTable("TelemetryFailures", "telemetry");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasColumnType("bigint").IsRequired().UseIdentityColumn();
        b.Property(e => e.EventId).HasColumnType("uniqueidentifier").IsRequired();
        b.Property(e => e.TimestampUtc).HasColumnType("datetime2(3)").IsRequired();
        b.Property(e => e.Error).HasColumnType("nvarchar(200)").HasMaxLength(200).IsRequired();
        b.Property(e => e.Attempt).HasColumnType("int").IsRequired();
        b.HasOne<TelemetryEventEntity>().WithMany().HasForeignKey(e => e.EventId).HasPrincipalKey(e => e.EventId).OnDelete(DeleteBehavior.NoAction);
    }
}
