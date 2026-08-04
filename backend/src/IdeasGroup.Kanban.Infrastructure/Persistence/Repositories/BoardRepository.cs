using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdeasGroup.Kanban.Infrastructure.Persistence.Repositories;

public class BoardRepository : IBoardRepository
{
    private readonly KanbanDbContext _dbContext;

    public BoardRepository(KanbanDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Board?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        _dbContext.Boards
            .Include(b => b.Columns)
            .ThenInclude(c => c.Tasks)
            .FirstOrDefaultAsync(b => b.ProjectId == projectId, cancellationToken);

    public Task UpdateAsync(Board board, CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    // Ver el comentario en ProjectRepository.AddMemberAsync: una columna nueva agregada a
    // un Board ya rastreado necesita un Add() explícito, no basta con dejar que
    // UpdateAsync/SaveChanges la detecte sola.
    public async Task AddColumnAsync(BoardColumn column, CancellationToken cancellationToken = default)
    {
        _dbContext.BoardColumns.Add(column);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
