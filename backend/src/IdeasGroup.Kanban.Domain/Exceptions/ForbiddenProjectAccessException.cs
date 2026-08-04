namespace IdeasGroup.Kanban.Domain.Exceptions;

public class ForbiddenProjectAccessException : DomainException
{
    public ForbiddenProjectAccessException()
        : base("No tienes permisos suficientes sobre este proyecto.")
    {
    }
}
