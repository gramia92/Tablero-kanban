namespace IdeasGroup.Kanban.Domain.Exceptions;

public class BoardColumnNotFoundException : DomainException
{
    public BoardColumnNotFoundException()
        : base("La columna no existe.")
    {
    }
}
