using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdeasGroup.Kanban.Infrastructure.Persistence.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly KanbanDbContext _dbContext;

    public TaskRepository(KanbanDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<KanbanTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default) =>
        _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

    public async Task<IReadOnlyList<KanbanTask>> ListByColumnAsync(Guid boardColumnId, CancellationToken cancellationToken = default) =>
        await _dbContext.Tasks
            .Where(t => t.BoardColumnId == boardColumnId)
            .OrderBy(t => t.Position)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<KanbanTask>> ListByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default) =>
        await (from t in _dbContext.Tasks
               join c in _dbContext.BoardColumns on t.BoardColumnId equals c.Id
               where c.BoardId == boardId
               orderby t.Position
               select t)
            .ToListAsync(cancellationToken);

    public Task<double?> GetLastPositionAsync(Guid boardColumnId, CancellationToken cancellationToken = default) =>
        _dbContext.Tasks
            .Where(t => t.BoardColumnId == boardColumnId)
            .OrderByDescending(t => t.Position)
            .Select(t => (double?)t.Position)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(KanbanTask task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(KanbanTask task, CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    public Task UpdateRangeAsync(IReadOnlyList<KanbanTask> tasks, CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(KanbanTask task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
