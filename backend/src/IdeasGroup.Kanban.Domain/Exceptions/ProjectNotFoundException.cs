namespace IdeasGroup.Kanban.Domain.Exceptions;

public class ProjectNotFoundException : DomainException
{
    public ProjectNotFoundException()
        : base("El proyecto no existe.")
    {
    }
}
