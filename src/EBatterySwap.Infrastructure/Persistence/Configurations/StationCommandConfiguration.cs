using EBatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBatterySwap.Infrastructure.Persistence.Configurations;

internal sealed class StationCommandConfiguration : IEntityTypeConfiguration<StationCommand>
{
    public void Configure(EntityTypeBuilder<StationCommand> builder)
    {
        builder.ToTable("StationCommand");
        builder.HasKey(x => x.CommandId).HasName("PK_StationCommand");

        builder.Property(x => x.CommandType).HasMaxLength(30).IsUnicode(false).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Status).HasEnumAsText(20);
        builder.Property(x => x.CreatedAt).HasDateTime3();
        builder.Property(x => x.SentAt).HasDateTime3();
        builder.Property(x => x.CompletedAt).HasDateTime3();
        builder.Property(x => x.RetryCount).HasDefaultValue((byte)0);

        builder.HasIndex(x => new { x.StationId, x.CreatedAt }, "IX_StationCommand_Station_CreatedAt")
            .IsDescending(false, true);

        builder.HasOne(x => x.Station)
            .WithMany()
            .HasForeignKey(x => x.StationId)
            .HasConstraintName("FK_StationCommand_Station")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Slot)
            .WithMany()
            .HasForeignKey(x => x.SlotId)
            .HasConstraintName("FK_StationCommand_Slot")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SwapItem)
            .WithMany()
            .HasForeignKey(x => x.SwapItemId)
            .HasConstraintName("FK_StationCommand_SwapItem")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ChargingSession)
            .WithMany()
            .HasForeignKey(x => x.ChargingSessionId)
            .HasConstraintName("FK_StationCommand_ChargingSession")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
