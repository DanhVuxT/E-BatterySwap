using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EBatterySwap.Infrastructure.Persistence.Converters;

/// <summary>
/// Converts an enum to the upper snake case text stored in SQL Server,
/// for example <c>InProgress</c> to <c>IN_PROGRESS</c>.
/// </summary>
public sealed class SnakeUpperEnumConverter<TEnum>()
    : ValueConverter<TEnum, string>(
        value => EnumText<TEnum>.ToText(value),
        text => EnumText<TEnum>.FromText(text))
    where TEnum : struct, Enum;

public static class EnumText<TEnum> where TEnum : struct, Enum
{
    private static readonly Dictionary<TEnum, string> ToMap = new();
    private static readonly Dictionary<string, TEnum> FromMap = new(StringComparer.Ordinal);

    static EnumText()
    {
        foreach (var value in Enum.GetValues<TEnum>())
        {
            var text = ToSnakeUpper(value.ToString());
            ToMap[value] = text;
            FromMap[text] = value;
        }
    }

    public static string ToText(TEnum value) => ToMap[value];

    public static TEnum FromText(string text) =>
        FromMap.TryGetValue(text, out var value)
            ? value
            : throw new InvalidOperationException(
                $"'{text}' is not a valid value for {typeof(TEnum).Name}.");

    private static string ToSnakeUpper(string name)
    {
        var builder = new StringBuilder(name.Length + 4);

        for (var i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]))
            {
                builder.Append('_');
            }

            builder.Append(char.ToUpperInvariant(name[i]));
        }

        return builder.ToString();
    }
}
