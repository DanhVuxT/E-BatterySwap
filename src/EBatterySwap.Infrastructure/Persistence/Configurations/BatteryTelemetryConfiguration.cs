using EBatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBatterySwap.Infrastructure.Persistence.Configurations;

internal sealed class BatteryTelemetryConfiguration : IEntityTypeConfiguration<BatteryTelemetry>
{
    public void Configure(EntityTypeBuilder<BatteryTelemetry> builder)
    {
        builder.ToTable("BatteryTelemetry");
        builder.HasKey(x => x.TelemetryId).HasName("PK_BatteryTelemetry");

        builder.Property(x => x.Soc).HasColumnName("SOC").HasDecimal52();
        builder.Property(x => x.Soh).HasColumnName("SOH").HasDecimal52();
        builder.Property(x => x.Temperature).HasDecimal52();
        builder.Property(x => x.MeasuredAt).HasDateTime3();
        builder.Property(x => x.ReceivedAt).HasDateTime3();
        builder.Property(x => x.Source).HasEnumAsText(20);

        builder.HasIndex(
                x => new { x.BatteryId, x.MeasuredAt },
                "IX_BatteryTelemetry_Battery_MeasuredAt")
            .IsDescending(false, true);

        builder.HasOne(x => x.Battery)
            .WithMany()
            .HasForeignKey(x => x.BatteryId)
            .HasConstraintName("FK_BatteryTelemetry_Battery")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
