using IdeasGroup.Kanban.Domain.Entities;

namespace IdeasGroup.Kanban.Application.Abstractions;

public interface ITaskRepository
{
    Task<KanbanTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KanbanTask>> ListByColumnAsync(Guid boardColumnId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KanbanTask>> ListByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default);

    Task<double?> GetLastPositionAsync(Guid boardColumnId, CancellationToken cancellationToken = default);

    Task AddAsync(KanbanTask task, CancellationToken cancellationToken = default);

    Task UpdateAsync(KanbanTask task, CancellationToken cancellationToken = default);

    Task UpdateRangeAsync(IReadOnlyList<KanbanTask> tasks, CancellationToken cancellationToken = default);

    Task DeleteAsync(KanbanTask task, CancellationToken cancellationToken = default);
}
