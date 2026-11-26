using EBatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBatterySwap.Infrastructure.Persistence.Configurations;

internal sealed class SlotConfiguration : IEntityTypeConfiguration<Slot>
{
    public void Configure(EntityTypeBuilder<Slot> builder)
    {
        builder.ToTable("Slot");
        builder.HasKey(x => x.SlotId).HasName("PK_Slot");

        builder.Property(x => x.SlotNumber).IsRequired();
        builder.Property(x => x.DoorStatus).HasEnumAsText(20);
        builder.Property(x => x.LockStatus).HasEnumAsText(20);
        builder.Property(x => x.OperationalStatus).HasEnumAsText(20);
        builder.Property(x => x.CreatedAt).HasDateTime3();

        builder.HasIndex(x => new { x.StationId, x.SlotNumber }, "UQ_Slot_Station_SlotNumber")
            .IsUnique();

        builder.HasOne(x => x.Station)
            .WithMany(x => x.Slots)
            .HasForeignKey(x => x.StationId)
            .HasConstraintName("FK_Slot_Station")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
