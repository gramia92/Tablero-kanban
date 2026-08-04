using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Domain.Entities;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Application.Reports;

public class ReportService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly IReadOnlyDictionary<ReportFormat, IReportExporter> _exporters;

    public ReportService(
        IProjectRepository projectRepository,
        IBoardRepository boardRepository,
        ITaskRepository taskRepository,
        IUserRepository userRepository,
        IEnumerable<IReportExporter> exporters)
    {
        _projectRepository = projectRepository;
        _boardRepository = boardRepository;
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _exporters = exporters.ToDictionary(e => e.Format);
    }

    public async Task<ExportedFile> ExportProjectReportAsync(Guid userId, Guid projectId, ReportFormat format, CancellationToken cancellationToken = default)
    {
        if (!_exporters.TryGetValue(format, out var exporter))
        {
            throw new DomainException($"El formato de exportación '{format}' no está soportado.");
        }

        var report = await BuildReportAsync(userId, projectId, cancellationToken);

        return exporter.Export(report);
    }

    private async Task<ProjectReport> BuildReportAsync(Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ProjectNotFoundException();

        if (project.Members.All(m => m.UserId != userId))
        {
            throw new ForbiddenProjectAccessException();
        }

        var board = await _boardRepository.GetByProjectIdAsync(projectId, cancellationToken)
            ?? throw new BoardNotFoundException();

        var tasks = await _taskRepository.ListByBoardIdAsync(board.Id, cancellationToken);
        var columnsById = board.Columns.ToDictionary(c => c.Id);

        var assigneeIds = tasks.Where(t => t.AssigneeId.HasValue).Select(t => t.AssigneeId!.Value).Distinct();
        var usersById = (await _userRepository.GetByIdsAsync(assigneeIds, cancellationToken)).ToDictionary(u => u.Id);

        var rows = tasks
            .OrderBy(t => columnsById[t.BoardColumnId].Order)
            .ThenBy(t => t.Position)
            .Select(t => BuildRow(t, columnsById, usersById))
            .ToList();

        return new ProjectReport(project.Name, DateTime.UtcNow, rows);
    }

    private static ProjectReportRow BuildRow(
        KanbanTask task,
        IReadOnlyDictionary<Guid, BoardColumn> columnsById,
        IReadOnlyDictionary<Guid, User> usersById)
    {
        string? assigneeName = null;
        if (task.AssigneeId.HasValue && usersById.TryGetValue(task.AssigneeId.Value, out var assignee))
        {
            assigneeName = assignee.FullName;
        }

        return new ProjectReportRow(
            columnsById[task.BoardColumnId].Name,
            task.Title,
            task.Description,
            task.Priority.ToString(),
            assigneeName,
            task.DueDate,
            task.CreatedAt);
    }
}
