using EBatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBatterySwap.Infrastructure.Persistence.Configurations;

internal sealed class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("Alert");
        builder.HasKey(x => x.AlertId).HasName("PK_Alert");

        builder.Property(x => x.AlertType).HasMaxLength(40).IsUnicode(false).IsRequired();
        builder.Property(x => x.Severity).HasEnumAsText(20);
        builder.Property(x => x.Title).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Status).HasEnumAsText(20);
        builder.Property(x => x.CreatedAt).HasDateTime3();
        builder.Property(x => x.AcknowledgedAt).HasDateTime3();
        builder.Property(x => x.ResolvedAt).HasDateTime3();

        builder.HasIndex(
                x => new { x.StationId, x.Status, x.CreatedAt },
                "IX_Alert_Station_Status_CreatedAt")
            .IsDescending(false, false, true);

        builder.HasOne(x => x.Station)
            .WithMany()
            .HasForeignKey(x => x.StationId)
            .HasConstraintName("FK_Alert_Station")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Event)
            .WithMany()
            .HasForeignKey(x => x.EventId)
            .HasConstraintName("FK_Alert_Event")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AcknowledgedByUser)
            .WithMany()
            .HasForeignKey(x => x.AcknowledgedByUserId)
            .HasConstraintName("FK_Alert_AcknowledgedByUser")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ResolvedByUser)
            .WithMany()
            .HasForeignKey(x => x.ResolvedByUserId)
            .HasConstraintName("FK_Alert_ResolvedByUser")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
