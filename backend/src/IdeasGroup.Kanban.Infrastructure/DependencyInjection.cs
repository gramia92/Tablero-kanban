using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Infrastructure.Persistence;
using IdeasGroup.Kanban.Infrastructure.Persistence.Repositories;
using IdeasGroup.Kanban.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdeasGroup.Kanban.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<KanbanDbContext>(options => options.UseNpgsql(connectionString));

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
