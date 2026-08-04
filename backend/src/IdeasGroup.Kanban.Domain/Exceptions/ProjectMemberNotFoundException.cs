namespace IdeasGroup.Kanban.Domain.Exceptions;

public class ProjectMemberNotFoundException : DomainException
{
    public ProjectMemberNotFoundException()
        : base("El usuario no es miembro de este proyecto.")
    {
    }
}
