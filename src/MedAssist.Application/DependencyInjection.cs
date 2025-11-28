using Microsoft.Extensions.DependencyInjection;

namespace MedAssist.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
