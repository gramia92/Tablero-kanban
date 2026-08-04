using IdeasGroup.Kanban.Domain.Enums;

namespace IdeasGroup.Kanban.Application.Tasks;

public record TaskResponse(
    Guid Id,
    Guid BoardColumnId,
    string Title,
    string? Description,
    double Position,
    Priority Priority,
    Guid? AssigneeId,
    string? AssigneeName,
    Guid CreatedById,
    DateTime? DueDate,
    DateTime CreatedAtUtc);
