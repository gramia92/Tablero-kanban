using IdeasGroup.Kanban.Domain.Enums;

namespace IdeasGroup.Kanban.Application.Projects;

public record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerId,
    DateTime? StartDate,
    DateTime? ExpectedEndDate,
    ProjectStatus Status,
    DateTime CreatedAtUtc,
    IReadOnlyList<ProjectMemberResponse> Members);
