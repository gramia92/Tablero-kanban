using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Domain.Entities;

namespace IdeasGroup.Kanban.Application.Tests.Tasks;

public class FakeTaskRepository : ITaskRepository
{
    private readonly List<KanbanTask> _tasks = new();

    public FakeTaskRepository Seed(KanbanTask task)
    {
        _tasks.Add(task);
        return this;
    }

    public Task<KanbanTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tasks.SingleOrDefault(t => t.Id == taskId));

    public Task<IReadOnlyList<KanbanTask>> ListByColumnAsync(Guid boardColumnId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<KanbanTask>>(
            _tasks.Where(t => t.BoardColumnId == boardColumnId).OrderBy(t => t.Position).ToList());

    public Task<IReadOnlyList<KanbanTask>> ListByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<KanbanTask>>(_tasks.OrderBy(t => t.Position).ToList());

    public Task<double?> GetLastPositionAsync(Guid boardColumnId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tasks.Where(t => t.BoardColumnId == boardColumnId)
            .OrderByDescending(t => t.Position)
            .Select(t => (double?)t.Position)
            .FirstOrDefault());

    public Task AddAsync(KanbanTask task, CancellationToken cancellationToken = default)
    {
        _tasks.Add(task);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(KanbanTask task, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task UpdateRangeAsync(IReadOnlyList<KanbanTask> tasks, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeleteAsync(KanbanTask task, CancellationToken cancellationToken = default)
    {
        _tasks.Remove(task);
        return Task.CompletedTask;
    }
}
