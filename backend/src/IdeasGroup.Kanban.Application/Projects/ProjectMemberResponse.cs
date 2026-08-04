namespace IdeasGroup.Kanban.Application.Projects;

public record ProjectMemberResponse(Guid UserId, string FullName, string Email, string Role);
