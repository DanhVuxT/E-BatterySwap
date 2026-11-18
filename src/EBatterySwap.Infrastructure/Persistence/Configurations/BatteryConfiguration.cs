using EBatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBatterySwap.Infrastructure.Persistence.Configurations;

internal sealed class BatteryConfiguration : IEntityTypeConfiguration<Battery>
{
    public void Configure(EntityTypeBuilder<Battery> builder)
    {
        builder.ToTable("Battery");
        builder.HasKey(x => x.BatteryId).HasName("PK_Battery");

        builder.Property(x => x.SerialNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Soc).HasColumnName("SOC").HasDecimal52();
        builder.Property(x => x.Soh).HasColumnName("SOH").HasDecimal52();
        builder.Property(x => x.Temperature).HasDecimal52();
        builder.Property(x => x.CycleCount).IsRequired();
        builder.Property(x => x.OperationalStatus).HasEnumAsText(20);
        builder.Property(x => x.LocationType).HasEnumAsText(20);
        builder.Property(x => x.LastTelemetryAt).HasDateTime3();
        builder.Property(x => x.CreatedAt).HasDateTime3();

        builder.HasIndex(x => x.SerialNumber, "UQ_Battery_SerialNumber").IsUnique();

        // A slot holds at most one battery at a time.
        builder.HasIndex(x => x.CurrentSlotId, "UX_Battery_CurrentSlotId_NotNull")
            .IsUnique()
            .HasFilter("[CurrentSlotId] IS NOT NULL");

        builder.HasOne(x => x.CurrentSlot)
            .WithMany()
            .HasForeignKey(x => x.CurrentSlotId)
            .HasConstraintName("FK_Battery_CurrentSlot")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
