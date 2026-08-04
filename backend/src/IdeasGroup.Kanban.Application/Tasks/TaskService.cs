using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Domain.Entities;
using IdeasGroup.Kanban.Domain.Exceptions;
using IdeasGroup.Kanban.Domain.Services;

namespace IdeasGroup.Kanban.Application.Tasks;

public class TaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBoardRealtimeNotifier _notifier;

    public TaskService(
        ITaskRepository taskRepository,
        IBoardRepository boardRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IBoardRealtimeNotifier notifier)
    {
        _taskRepository = taskRepository;
        _boardRepository = boardRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _notifier = notifier;
    }

    public async Task<IReadOnlyList<TaskResponse>> ListByProjectAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var board = await EnsureMemberAndGetBoardAsync(userId, projectId, cancellationToken);
        var tasks = await _taskRepository.ListByBoardIdAsync(board.Id, cancellationToken);
        var usersById = await GetUsersByIdAsync(tasks, cancellationToken);

        return tasks.Select(t => BuildResponse(t, usersById)).ToList();
    }

    public async Task<TaskResponse> CreateAsync(Guid userId, Guid projectId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var board = await EnsureMemberAndGetBoardAsync(userId, projectId, cancellationToken);
        EnsureColumnBelongsToBoard(board, request.BoardColumnId);

        var lastPosition = await _taskRepository.GetLastPositionAsync(request.BoardColumnId, cancellationToken);
        var position = TaskPositionCalculator.GetPositionForAppend(lastPosition);

        var task = KanbanTask.Create(
            request.BoardColumnId,
            request.Title,
            request.Description,
            position,
            request.Priority,
            userId,
            request.AssigneeId,
            request.DueDate);

        await _taskRepository.AddAsync(task, cancellationToken);

        var response = await ToResponseAsync(task, cancellationToken);
        await _notifier.NotifyTaskCreatedAsync(projectId, response, cancellationToken);

        return response;
    }

    public async Task<TaskResponse> UpdateAsync(Guid userId, Guid projectId, Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var board = await EnsureMemberAndGetBoardAsync(userId, projectId, cancellationToken);
        var task = await GetTaskOrThrowAsync(taskId, cancellationToken);
        EnsureColumnBelongsToBoard(board, task.BoardColumnId);

        task.Update(request.Title, request.Description, request.Priority, request.AssigneeId, request.DueDate);
        await _taskRepository.UpdateAsync(task, cancellationToken);

        var response = await ToResponseAsync(task, cancellationToken);
        await _notifier.NotifyTaskUpdatedAsync(projectId, response, cancellationToken);

        return response;
    }

    public async Task DeleteAsync(Guid userId, Guid projectId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var board = await EnsureMemberAndGetBoardAsync(userId, projectId, cancellationToken);
        var task = await GetTaskOrThrowAsync(taskId, cancellationToken);
        EnsureColumnBelongsToBoard(board, task.BoardColumnId);

        await _taskRepository.DeleteAsync(task, cancellationToken);
        await _notifier.NotifyTaskDeletedAsync(projectId, task.Id, cancellationToken);
    }

    public async Task<TaskResponse> MoveAsync(Guid userId, Guid projectId, Guid taskId, MoveTaskRequest request, CancellationToken cancellationToken = default)
    {
        var board = await EnsureMemberAndGetBoardAsync(userId, projectId, cancellationToken);
        var task = await GetTaskOrThrowAsync(taskId, cancellationToken);
        EnsureColumnBelongsToBoard(board, task.BoardColumnId);
        EnsureColumnBelongsToBoard(board, request.TargetColumnId);

        var siblings = (await _taskRepository.ListByColumnAsync(request.TargetColumnId, cancellationToken))
            .Where(t => t.Id != task.Id)
            .OrderBy(t => t.Position)
            .ToList();

        var insertIndex = ResolveInsertIndex(siblings, request.PreviousTaskId, request.NextTaskId);
        var previousPosition = insertIndex > 0 ? siblings[insertIndex - 1].Position : (double?)null;
        var nextPosition = insertIndex < siblings.Count ? siblings[insertIndex].Position : (double?)null;

        try
        {
            var position = TaskPositionCalculator.CalculatePosition(previousPosition, nextPosition);
            task.MoveTo(request.TargetColumnId, position);
            await _taskRepository.UpdateAsync(task, cancellationToken);
        }
        catch (RebalanceRequiredException)
        {
            siblings.Insert(insertIndex, task);
            var positions = TaskPositionCalculator.Rebalance(siblings.Count);
            for (var i = 0; i < siblings.Count; i++)
            {
                siblings[i].MoveTo(request.TargetColumnId, positions[i]);
            }

            await _taskRepository.UpdateRangeAsync(siblings, cancellationToken);
        }

        var response = await ToResponseAsync(task, cancellationToken);
        await _notifier.NotifyTaskMovedAsync(projectId, response, cancellationToken);

        return response;
    }

    private static int ResolveInsertIndex(IReadOnlyList<KanbanTask> orderedSiblings, Guid? previousTaskId, Guid? nextTaskId)
    {
        if (nextTaskId is { } next)
        {
            var index = IndexOf(orderedSiblings, next);
            if (index >= 0)
            {
                return index;
            }
        }

        if (previousTaskId is { } previous)
        {
            var index = IndexOf(orderedSiblings, previous);
            if (index >= 0)
            {
                return index + 1;
            }
        }

        return orderedSiblings.Count;
    }

    private static int IndexOf(IReadOnlyList<KanbanTask> tasks, Guid taskId)
    {
        for (var i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].Id == taskId)
            {
                return i;
            }
        }

        return -1;
    }

    private async Task<Board> EnsureMemberAndGetBoardAsync(Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ProjectNotFoundException();

        if (project.Members.All(m => m.UserId != userId))
        {
            throw new ForbiddenProjectAccessException();
        }

        return await _boardRepository.GetByProjectIdAsync(projectId, cancellationToken)
            ?? throw new BoardNotFoundException();
    }

    private static void EnsureColumnBelongsToBoard(Board board, Guid columnId)
    {
        if (board.Columns.All(c => c.Id != columnId))
        {
            throw new BoardColumnNotFoundException();
        }
    }

    private async Task<KanbanTask> GetTaskOrThrowAsync(Guid taskId, CancellationToken cancellationToken) =>
        await _taskRepository.GetByIdAsync(taskId, cancellationToken)
            ?? throw new TaskNotFoundException();

    private async Task<TaskResponse> ToResponseAsync(KanbanTask task, CancellationToken cancellationToken)
    {
        var usersById = await GetUsersByIdAsync(new[] { task }, cancellationToken);
        return BuildResponse(task, usersById);
    }

    private async Task<Dictionary<Guid, User>> GetUsersByIdAsync(IEnumerable<KanbanTask> tasks, CancellationToken cancellationToken)
    {
        var assigneeIds = tasks.Where(t => t.AssigneeId.HasValue).Select(t => t.AssigneeId!.Value).Distinct();
        var users = await _userRepository.GetByIdsAsync(assigneeIds, cancellationToken);
        return users.ToDictionary(u => u.Id);
    }

    private static TaskResponse BuildResponse(KanbanTask task, IReadOnlyDictionary<Guid, User> usersById)
    {
        string? assigneeName = null;
        if (task.AssigneeId.HasValue && usersById.TryGetValue(task.AssigneeId.Value, out var assignee))
        {
            assigneeName = assignee.FullName;
        }

        return new TaskResponse(
            task.Id,
            task.BoardColumnId,
            task.Title,
            task.Description,
            task.Position,
            task.Priority,
            task.AssigneeId,
            assigneeName,
            task.CreatedById,
            task.DueDate,
            task.CreatedAt);
    }
}
