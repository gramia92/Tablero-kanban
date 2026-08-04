using IdeasGroup.Kanban.Domain.Enums;

namespace IdeasGroup.Kanban.Application.Tasks;

public record CreateTaskRequest(
    Guid BoardColumnId,
    string Title,
    string? Description,
    Priority Priority,
    Guid? AssigneeId,
    DateTime? DueDate);
