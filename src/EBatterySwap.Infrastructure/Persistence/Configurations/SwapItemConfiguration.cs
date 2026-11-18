using EBatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBatterySwap.Infrastructure.Persistence.Configurations;

internal sealed class SwapItemConfiguration : IEntityTypeConfiguration<SwapItem>
{
    public void Configure(EntityTypeBuilder<SwapItem> builder)
    {
        builder.ToTable("SwapItem");
        builder.HasKey(x => x.SwapItemId).HasName("PK_SwapItem");

        builder.Property(x => x.SequenceNumber).IsRequired();
        builder.Property(x => x.Status).HasEnumAsText(30);
        builder.Property(x => x.ReturnedAt).HasDateTime3();
        builder.Property(x => x.IssuedAt).HasDateTime3();
        builder.Property(x => x.FailureReason).HasMaxLength(255);
        builder.Property(x => x.CreatedAt).HasDateTime3();

        builder.HasIndex(x => new { x.SwapSessionId, x.SequenceNumber }, "UQ_SwapItem_Session_Sequence")
            .IsUnique();
        builder.HasIndex(x => x.ReturnedBatteryId, "IX_SwapItem_ReturnedBatteryId")
            .HasFilter("[ReturnedBatteryId] IS NOT NULL");
        builder.HasIndex(x => x.IssuedBatteryId, "IX_SwapItem_IssuedBatteryId")
            .HasFilter("[IssuedBatteryId] IS NOT NULL");

        builder.HasOne(x => x.SwapSession)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.SwapSessionId)
            .HasConstraintName("FK_SwapItem_SwapSession")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReturnedBattery)
            .WithMany()
            .HasForeignKey(x => x.ReturnedBatteryId)
            .HasConstraintName("FK_SwapItem_ReturnedBattery")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.IssuedBattery)
            .WithMany()
            .HasForeignKey(x => x.IssuedBatteryId)
            .HasConstraintName("FK_SwapItem_IssuedBattery")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReturnSlot)
            .WithMany()
            .HasForeignKey(x => x.ReturnSlotId)
            .HasConstraintName("FK_SwapItem_ReturnSlot")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.IssueSlot)
            .WithMany()
            .HasForeignKey(x => x.IssueSlotId)
            .HasConstraintName("FK_SwapItem_IssueSlot")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
