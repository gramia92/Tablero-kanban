using IdeasGroup.Kanban.Domain.Enums;

namespace IdeasGroup.Kanban.Application.Projects;

public record CreateProjectRequest(
    string Name,
    string? Description,
    DateTime? StartDate = null,
    DateTime? ExpectedEndDate = null,
    ProjectStatus Status = ProjectStatus.Planned);
