namespace IdeasGroup.Kanban.Domain.Exceptions;

public class ColumnHasTasksException : DomainException
{
    public ColumnHasTasksException()
        : base("No se puede eliminar una columna que contiene tareas.")
    {
    }
}
