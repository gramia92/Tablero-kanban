using IdeasGroup.Kanban.Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace IdeasGroup.Kanban.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();

        return services;
    }
}
