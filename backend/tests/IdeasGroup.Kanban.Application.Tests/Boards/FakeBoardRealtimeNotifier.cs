using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Application.Boards;
using IdeasGroup.Kanban.Application.Tasks;

namespace IdeasGroup.Kanban.Application.Tests.Boards;

public class FakeBoardRealtimeNotifier : IBoardRealtimeNotifier
{
    public Task NotifyTaskCreatedAsync(Guid projectId, TaskResponse task, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task NotifyTaskUpdatedAsync(Guid projectId, TaskResponse task, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task NotifyTaskMovedAsync(Guid projectId, TaskResponse task, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task NotifyTaskDeletedAsync(Guid projectId, Guid taskId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task NotifyBoardChangedAsync(Guid projectId, BoardResponse board, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
