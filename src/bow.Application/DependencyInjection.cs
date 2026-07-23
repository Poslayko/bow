using bow.Application.Users.Register;
using Microsoft.Extensions.DependencyInjection;

namespace bow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserHandler>();

        return services;
    }
}