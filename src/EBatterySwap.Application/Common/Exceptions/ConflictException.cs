namespace EBatterySwap.Application.Common.Exceptions;

public sealed class ConflictException(string message)
    : AppException(message, 409, "Conflict");
