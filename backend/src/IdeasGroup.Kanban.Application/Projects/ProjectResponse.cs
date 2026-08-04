namespace IdeasGroup.Kanban.Application.Projects;

public record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerId,
    DateTime CreatedAtUtc,
    IReadOnlyList<ProjectMemberResponse> Members);
