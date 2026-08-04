using IdeasGroup.Kanban.Application.Auth;
using IdeasGroup.Kanban.Application.Boards;
using IdeasGroup.Kanban.Application.Projects;
using IdeasGroup.Kanban.Application.Reports;
using IdeasGroup.Kanban.Application.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace IdeasGroup.Kanban.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<BoardService>();
        services.AddScoped<TaskService>();
        services.AddScoped<ReportService>();

        return services;
    }
}
