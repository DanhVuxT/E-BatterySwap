using EBatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBatterySwap.Infrastructure.Persistence.Configurations;

internal sealed class SwapSessionConfiguration : IEntityTypeConfiguration<SwapSession>
{
    public void Configure(EntityTypeBuilder<SwapSession> builder)
    {
        builder.ToTable("SwapSession");
        builder.HasKey(x => x.SwapSessionId).HasName("PK_SwapSession");

        builder.Property(x => x.RequestedBatteryCount).IsRequired();
        builder.Property(x => x.Status).HasEnumAsText(30);
        builder.Property(x => x.StartedAt).HasDateTime3();
        builder.Property(x => x.EndedAt).HasDateTime3();
        builder.Property(x => x.FailureReason).HasMaxLength(255);

        // A vehicle can have only one open swap session.
        builder.HasIndex(x => x.VehicleId, "UX_SwapSession_OneOpenPerVehicle")
            .IsUnique()
            .HasFilter("[EndedAt] IS NULL");
        builder.HasIndex(x => x.StationId, "IX_SwapSession_StationId");

        builder.HasOne(x => x.Vehicle)
            .WithMany()
            .HasForeignKey(x => x.VehicleId)
            .HasConstraintName("FK_SwapSession_Vehicle")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Station)
            .WithMany()
            .HasForeignKey(x => x.StationId)
            .HasConstraintName("FK_SwapSession_Station")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
