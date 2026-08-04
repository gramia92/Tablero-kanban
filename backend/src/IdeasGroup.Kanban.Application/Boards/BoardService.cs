using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Domain.Entities;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Application.Boards;

public class BoardService
{
    private readonly IBoardRepository _boardRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IBoardRealtimeNotifier _notifier;

    public BoardService(IBoardRepository boardRepository, IProjectRepository projectRepository, IBoardRealtimeNotifier notifier)
    {
        _boardRepository = boardRepository;
        _projectRepository = projectRepository;
        _notifier = notifier;
    }

    public async Task<BoardResponse> GetByProjectIdAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        await EnsureMemberAsync(userId, projectId, cancellationToken);
        var board = await GetBoardOrThrowAsync(projectId, cancellationToken);

        return BuildResponse(board);
    }

    public async Task<ColumnResponse> AddColumnAsync(Guid userId, Guid projectId, CreateColumnRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureMemberAsync(userId, projectId, cancellationToken);
        var board = await GetBoardOrThrowAsync(projectId, cancellationToken);

        var column = board.AddColumn(request.Name);
        await _boardRepository.AddColumnAsync(column, cancellationToken);

        await _notifier.NotifyBoardChangedAsync(projectId, BuildResponse(board), cancellationToken);

        return new ColumnResponse(column.Id, column.Name, column.Order, 0);
    }

    public async Task<BoardResponse> RenameColumnAsync(Guid userId, Guid projectId, Guid columnId, RenameColumnRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureMemberAsync(userId, projectId, cancellationToken);
        var board = await GetBoardOrThrowAsync(projectId, cancellationToken);

        board.RenameColumn(columnId, request.Name);
        await _boardRepository.UpdateAsync(board, cancellationToken);

        var response = BuildResponse(board);
        await _notifier.NotifyBoardChangedAsync(projectId, response, cancellationToken);

        return response;
    }

    public async Task<BoardResponse> DeleteColumnAsync(Guid userId, Guid projectId, Guid columnId, CancellationToken cancellationToken = default)
    {
        await EnsureMemberAsync(userId, projectId, cancellationToken);
        var board = await GetBoardOrThrowAsync(projectId, cancellationToken);

        board.RemoveColumn(columnId);
        await _boardRepository.UpdateAsync(board, cancellationToken);

        var response = BuildResponse(board);
        await _notifier.NotifyBoardChangedAsync(projectId, response, cancellationToken);

        return response;
    }

    public async Task<BoardResponse> ReorderColumnsAsync(Guid userId, Guid projectId, ReorderColumnsRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureMemberAsync(userId, projectId, cancellationToken);
        var board = await GetBoardOrThrowAsync(projectId, cancellationToken);

        board.ReorderColumns(request.OrderedColumnIds);
        await _boardRepository.UpdateAsync(board, cancellationToken);

        var response = BuildResponse(board);
        await _notifier.NotifyBoardChangedAsync(projectId, response, cancellationToken);

        return response;
    }

    private async Task EnsureMemberAsync(Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ProjectNotFoundException();

        if (project.Members.All(m => m.UserId != userId))
        {
            throw new ForbiddenProjectAccessException();
        }
    }

    private async Task<Board> GetBoardOrThrowAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return await _boardRepository.GetByProjectIdAsync(projectId, cancellationToken)
            ?? throw new BoardNotFoundException();
    }

    private static BoardResponse BuildResponse(Board board)
    {
        var columns = board.Columns
            .OrderBy(c => c.Order)
            .Select(c => new ColumnResponse(c.Id, c.Name, c.Order, c.Tasks.Count))
            .ToList();

        return new BoardResponse(board.Id, board.ProjectId, board.Name, columns);
    }
}
