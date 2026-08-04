namespace IdeasGroup.Kanban.Domain.Exceptions;

public class BoardNotFoundException : DomainException
{
    public BoardNotFoundException()
        : base("El tablero del proyecto no existe.")
    {
    }
}
