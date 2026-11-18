namespace EBatterySwap.Application.Common.Exceptions;

public sealed class ValidationException : AppException
{
    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", 400, "Validation failed")
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}
