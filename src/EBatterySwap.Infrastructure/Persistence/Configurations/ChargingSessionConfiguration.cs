using EBatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBatterySwap.Infrastructure.Persistence.Configurations;

internal sealed class ChargingSessionConfiguration : IEntityTypeConfiguration<ChargingSession>
{
    public void Configure(EntityTypeBuilder<ChargingSession> builder)
    {
        builder.ToTable("ChargingSession");
        builder.HasKey(x => x.ChargingSessionId).HasName("PK_ChargingSession");

        builder.Property(x => x.StartedAt).HasDateTime3();
        builder.Property(x => x.EndedAt).HasDateTime3();
        builder.Property(x => x.StartSoc).HasColumnName("StartSOC").HasDecimal52();
        builder.Property(x => x.EndSoc).HasColumnName("EndSOC").HasDecimal52();
        builder.Property(x => x.Status).HasEnumAsText(20);
        builder.Property(x => x.StopReason).HasMaxLength(255);

        // A battery or a slot can have only one charging session in progress.
        builder.HasIndex(x => x.BatteryId, "UX_ChargingSession_ActiveBattery")
            .IsUnique()
            .HasFilter("[Status] = 'CHARGING'");
        builder.HasIndex(x => x.SlotId, "UX_ChargingSession_ActiveSlot")
            .IsUnique()
            .HasFilter("[Status] = 'CHARGING'");
        builder.HasIndex(x => x.SlotId, "IX_ChargingSession_SlotId");

        builder.HasOne(x => x.Battery)
            .WithMany()
            .HasForeignKey(x => x.BatteryId)
            .HasConstraintName("FK_ChargingSession_Battery")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Slot)
            .WithMany()
            .HasForeignKey(x => x.SlotId)
            .HasConstraintName("FK_ChargingSession_Slot")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
