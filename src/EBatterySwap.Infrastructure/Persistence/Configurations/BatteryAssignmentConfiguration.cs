using EBatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBatterySwap.Infrastructure.Persistence.Configurations;

internal sealed class BatteryAssignmentConfiguration : IEntityTypeConfiguration<BatteryAssignment>
{
    public void Configure(EntityTypeBuilder<BatteryAssignment> builder)
    {
        builder.ToTable("BatteryAssignment");
        builder.HasKey(x => x.AssignmentId).HasName("PK_BatteryAssignment");

        builder.Property(x => x.AssignedAt).HasDateTime3();
        builder.Property(x => x.EndedAt).HasDateTime3();
        builder.Property(x => x.AssignmentSource).HasEnumAsText(30);

        // A battery can have only one active assignment.
        builder.HasIndex(x => x.BatteryId, "UX_BatteryAssignment_ActiveBattery")
            .IsUnique()
            .HasFilter("[EndedAt] IS NULL");
        builder.HasIndex(x => x.VehicleId, "IX_BatteryAssignment_VehicleId");
        builder.HasIndex(x => x.RecordedByUserId, "IX_BatteryAssignment_RecordedByUserId")
            .HasFilter("[RecordedByUserId] IS NOT NULL");

        builder.HasOne(x => x.Battery)
            .WithMany()
            .HasForeignKey(x => x.BatteryId)
            .HasConstraintName("FK_BatteryAssignment_Battery")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Vehicle)
            .WithMany()
            .HasForeignKey(x => x.VehicleId)
            .HasConstraintName("FK_BatteryAssignment_Vehicle")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RecordedByUser)
            .WithMany()
            .HasForeignKey(x => x.RecordedByUserId)
            .HasConstraintName("FK_BatteryAssignment_RecordedByUser")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
