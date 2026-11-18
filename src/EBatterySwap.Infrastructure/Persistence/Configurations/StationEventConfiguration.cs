using EBatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBatterySwap.Infrastructure.Persistence.Configurations;

internal sealed class StationEventConfiguration : IEntityTypeConfiguration<StationEvent>
{
    public void Configure(EntityTypeBuilder<StationEvent> builder)
    {
        builder.ToTable("StationEvent");
        builder.HasKey(x => x.EventId).HasName("PK_StationEvent");

        builder.Property(x => x.EventIdentifier).HasMaxLength(100).IsUnicode(false).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(40).IsUnicode(false).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("nvarchar(max)");
        builder.Property(x => x.OccurredAt).HasDateTime3();
        builder.Property(x => x.ReceivedAt).HasDateTime3();
        builder.Property(x => x.ProcessingStatus).HasEnumAsText(20);
        builder.Property(x => x.ProcessedAt).HasDateTime3();
        builder.Property(x => x.ProcessingError).HasMaxLength(255);

        // The same simulator event is never recorded twice for one station.
        builder.HasIndex(
                x => new { x.StationId, x.EventIdentifier },
                "UQ_StationEvent_Station_EventIdentifier")
            .IsUnique();
        builder.HasIndex(x => new { x.StationId, x.ReceivedAt }, "IX_StationEvent_Station_ReceivedAt")
            .IsDescending(false, true);

        builder.HasOne(x => x.Station)
            .WithMany()
            .HasForeignKey(x => x.StationId)
            .HasConstraintName("FK_StationEvent_Station")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Command)
            .WithMany()
            .HasForeignKey(x => x.CommandId)
            .HasConstraintName("FK_StationEvent_Command")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Slot)
            .WithMany()
            .HasForeignKey(x => x.SlotId)
            .HasConstraintName("FK_StationEvent_Slot")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Battery)
            .WithMany()
            .HasForeignKey(x => x.BatteryId)
            .HasConstraintName("FK_StationEvent_Battery")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
