namespace EBatterySwap.Application.Common.Exceptions;

public sealed class NotFoundException(string message)
    : AppException(message, 404, "Resource not found")
{
    public NotFoundException(string entityName, object key)
        : this($"{entityName} '{key}' was not found.")
    {
    }
}
