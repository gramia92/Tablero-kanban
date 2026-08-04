using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Domain.Entities;

namespace IdeasGroup.Kanban.Application.Tests.Boards;

public class FakeBoardRepository : IBoardRepository
{
    private readonly List<Board> _boards = new();

    public FakeBoardRepository Seed(Board board)
    {
        _boards.Add(board);
        return this;
    }

    public Task<Board?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_boards.SingleOrDefault(b => b.ProjectId == projectId));

    public Task UpdateAsync(Board board, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task AddColumnAsync(BoardColumn column, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
