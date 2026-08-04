using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IdeasGroup.Kanban.WebApi.Hubs;

[Authorize]
public class BoardHub : Hub
{
    private readonly IProjectRepository _projectRepository;

    public BoardHub(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public static string GroupName(Guid projectId) => $"project:{projectId}";

    // El cliente llama esto tras conectarse y al cambiar de tablero; se valida membresía
    // acá (no solo en los controllers HTTP) porque cualquiera con un token válido podría
    // intentar unirse al grupo de SignalR de un proyecto ajeno.
    public async Task JoinProject(Guid projectId)
    {
        var userId = Context.User!.GetUserId();
        var project = await _projectRepository.GetByIdAsync(projectId, Context.ConnectionAborted)
            ?? throw new HubException("El proyecto no existe.");

        if (project.Members.All(m => m.UserId != userId))
        {
            throw new HubException("No tienes acceso a este proyecto.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(projectId));
    }

    public Task LeaveProject(Guid projectId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(projectId));
}
