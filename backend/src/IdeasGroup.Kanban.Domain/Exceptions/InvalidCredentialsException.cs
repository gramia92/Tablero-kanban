namespace IdeasGroup.Kanban.Domain.Exceptions;

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("Email o contraseña inválidos.")
    {
    }
}
