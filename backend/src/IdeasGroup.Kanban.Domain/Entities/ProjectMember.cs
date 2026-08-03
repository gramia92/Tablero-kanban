using IdeasGroup.Kanban.Domain.Common;
using IdeasGroup.Kanban.Domain.Enums;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Domain.Entities;

public class ProjectMember : Entity
{
    public Guid ProjectId { get; private set; }
    public Guid UserId { get; private set; }
    public ProjectRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;

    private ProjectMember()
    {
    }

    public static ProjectMember Create(Guid projectId, Guid userId, ProjectRole role)
    {
        if (projectId == Guid.Empty || userId == Guid.Empty)
        {
            throw new DomainException("Proyecto y usuario son obligatorios para asignar un miembro.");
        }

        return new ProjectMember
        {
            ProjectId = projectId,
            UserId = userId,
            Role = role
        };
    }

    public void ChangeRole(ProjectRole role)
    {
        Role = role;
    }
}
