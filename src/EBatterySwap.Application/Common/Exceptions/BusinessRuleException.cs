namespace EBatterySwap.Application.Common.Exceptions;

/// <summary>A request that is well formed but breaks a business rule.</summary>
public sealed class BusinessRuleException(string message)
    : AppException(message, 422, "Business rule violated");
