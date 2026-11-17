using Microsoft.Extensions.DependencyInjection;

namespace EBatterySwap.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services, validators, and other use-case dependencies here.
        return services;
    }
}
