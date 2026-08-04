using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Application.Boards;
using IdeasGroup.Kanban.Application.Tasks;
using IdeasGroup.Kanban.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace IdeasGroup.Kanban.WebApi.Realtime;

public class SignalRBoardNotifier : IBoardRealtimeNotifier
{
    private readonly IHubContext<BoardHub> _hubContext;

    public SignalRBoardNotifier(IHubContext<BoardHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyTaskCreatedAsync(Guid projectId, TaskResponse task, CancellationToken cancellationToken = default) =>
        Send(projectId, "TaskCreated", task, cancellationToken);

    public Task NotifyTaskUpdatedAsync(Guid projectId, TaskResponse task, CancellationToken cancellationToken = default) =>
        Send(projectId, "TaskUpdated", task, cancellationToken);

    public Task NotifyTaskMovedAsync(Guid projectId, TaskResponse task, CancellationToken cancellationToken = default) =>
        Send(projectId, "TaskMoved", task, cancellationToken);

    public Task NotifyTaskDeletedAsync(Guid projectId, Guid taskId, CancellationToken cancellationToken = default) =>
        Send(projectId, "TaskDeleted", new { taskId }, cancellationToken);

    public Task NotifyBoardChangedAsync(Guid projectId, BoardResponse board, CancellationToken cancellationToken = default) =>
        Send(projectId, "BoardChanged", board, cancellationToken);

    private Task Send(Guid projectId, string method, object payload, CancellationToken cancellationToken) =>
        _hubContext.Clients.Group(BoardHub.GroupName(projectId)).SendAsync(method, payload, cancellationToken);
}
