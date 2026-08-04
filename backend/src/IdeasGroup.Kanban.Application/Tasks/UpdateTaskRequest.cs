using IdeasGroup.Kanban.Domain.Enums;

namespace IdeasGroup.Kanban.Application.Tasks;

public record UpdateTaskRequest(
    string Title,
    string? Description,
    Priority Priority,
    Guid? AssigneeId,
    DateTime? DueDate);
