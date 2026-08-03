namespace IdeasGroup.Kanban.Domain.Exceptions;

public class EmailAlreadyRegisteredException : DomainException
{
    public EmailAlreadyRegisteredException()
        : base("Ya existe un usuario registrado con ese email.")
    {
    }
}
