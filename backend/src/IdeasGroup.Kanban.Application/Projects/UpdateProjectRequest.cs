using IdeasGroup.Kanban.Domain.Enums;

namespace IdeasGroup.Kanban.Application.Projects;

public record UpdateProjectRequest(
    string Name,
    string? Description,
    DateTime? StartDate,
    DateTime? ExpectedEndDate,
    ProjectStatus Status);
