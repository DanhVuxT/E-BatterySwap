using EBatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBatterySwap.Infrastructure.Persistence.Configurations;

internal sealed class StationConfiguration : IEntityTypeConfiguration<Station>
{
    public void Configure(EntityTypeBuilder<Station> builder)
    {
        builder.ToTable("Station");
        builder.HasKey(x => x.StationId).HasName("PK_Station");

        builder.Property(x => x.StationCode).HasMaxLength(30).IsUnicode(false).IsRequired();
        builder.Property(x => x.StationName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Latitude).HasColumnType("decimal(9,6)");
        builder.Property(x => x.Longitude).HasColumnType("decimal(9,6)");
        builder.Property(x => x.QrIdentifier)
            .HasColumnName("QRIdentifier")
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();
        builder.Property(x => x.OperationalStatus).HasEnumAsText(20);
        builder.Property(x => x.ConnectionStatus).HasEnumAsText(20);
        builder.Property(x => x.LastSeenAt).HasDateTime3();
        builder.Property(x => x.CreatedAt).HasDateTime3();

        builder.HasIndex(x => x.StationCode, "UQ_Station_StationCode").IsUnique();
        builder.HasIndex(x => x.QrIdentifier, "UQ_Station_QRIdentifier").IsUnique();
    }
}
