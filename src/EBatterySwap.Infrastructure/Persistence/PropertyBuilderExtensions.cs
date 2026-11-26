using EBatterySwap.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBatterySwap.Infrastructure.Persistence;

public static class PropertyBuilderExtensions
{
    /// <summary>Stores an enum as VARCHAR(<paramref name="maxLength"/>) in upper snake case.</summary>
    public static PropertyBuilder<TEnum> HasEnumAsText<TEnum>(
        this PropertyBuilder<TEnum> builder,
        int maxLength)
        where TEnum : struct, Enum
    {
        return builder
            .HasConversion(new SnakeUpperEnumConverter<TEnum>())
            .HasMaxLength(maxLength)
            .IsUnicode(false);
    }

    /// <summary>Maps a timestamp to DATETIME2(3).</summary>
    public static PropertyBuilder<T> HasDateTime3<T>(this PropertyBuilder<T> builder)
    {
        return builder.HasColumnType("datetime2(3)");
    }

    /// <summary>Maps a number to DECIMAL(5,2), used for SOC, SOH and temperature.</summary>
    public static PropertyBuilder<T> HasDecimal52<T>(this PropertyBuilder<T> builder)
    {
        return builder.HasColumnType("decimal(5,2)");
    }
}
