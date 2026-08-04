using IdeasGroup.Kanban.Application.Boards;
using IdeasGroup.Kanban.Application.Tasks;

namespace IdeasGroup.Kanban.Application.Abstractions;

public interface IBoardRealtimeNotifier
{
    Task NotifyTaskCreatedAsync(Guid projectId, TaskResponse task, CancellationToken cancellationToken = default);

    Task NotifyTaskUpdatedAsync(Guid projectId, TaskResponse task, CancellationToken cancellationToken = default);

    Task NotifyTaskMovedAsync(Guid projectId, TaskResponse task, CancellationToken cancellationToken = default);

    Task NotifyTaskDeletedAsync(Guid projectId, Guid taskId, CancellationToken cancellationToken = default);

    Task NotifyBoardChangedAsync(Guid projectId, BoardResponse board, CancellationToken cancellationToken = default);
}
