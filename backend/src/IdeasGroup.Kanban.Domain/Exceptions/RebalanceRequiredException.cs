namespace IdeasGroup.Kanban.Domain.Exceptions;

public class RebalanceRequiredException : DomainException
{
    public RebalanceRequiredException()
        : base("No queda espacio suficiente entre posiciones; la columna debe reindexarse.")
    {
    }
}
