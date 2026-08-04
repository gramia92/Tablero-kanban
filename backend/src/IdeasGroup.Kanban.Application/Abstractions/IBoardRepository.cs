using IdeasGroup.Kanban.Domain.Entities;

namespace IdeasGroup.Kanban.Application.Abstractions;

public interface IBoardRepository
{
    Task<Board?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task UpdateAsync(Board board, CancellationToken cancellationToken = default);

    Task AddColumnAsync(BoardColumn column, CancellationToken cancellationToken = default);
}
