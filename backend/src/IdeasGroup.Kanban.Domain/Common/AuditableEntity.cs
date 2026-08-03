namespace IdeasGroup.Kanban.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; protected set; }

    protected void Touch() => UpdatedAt = DateTime.UtcNow;
}
