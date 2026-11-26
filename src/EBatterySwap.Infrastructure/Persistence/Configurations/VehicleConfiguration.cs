using EBatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBatterySwap.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicle");
        builder.HasKey(x => x.VehicleId).HasName("PK_Vehicle");

        builder.Property(x => x.LicensePlate).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ModelName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.BatteryCapacityCount).IsRequired();
        builder.Property(x => x.Status).HasEnumAsText(20);
        builder.Property(x => x.CreatedAt).HasDateTime3();
        builder.Property(x => x.UpdatedAt).HasDateTime3();

        builder.HasIndex(x => x.LicensePlate, "UQ_Vehicle_LicensePlate").IsUnique();
        builder.HasIndex(x => x.UserId, "IX_Vehicle_UserId");

        builder.HasOne(x => x.User)
            .WithMany(x => x.Vehicles)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("FK_Vehicle_Users")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
