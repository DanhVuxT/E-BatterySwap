namespace EBatterySwap.Application.Common.Exceptions;

/// <summary>Base type for expected, business-level errors that map to a 4xx response.</summary>
public abstract class AppException(string message, int statusCode, string title)
    : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;
}
