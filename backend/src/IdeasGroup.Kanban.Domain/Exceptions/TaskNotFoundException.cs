namespace IdeasGroup.Kanban.Domain.Exceptions;

public class TaskNotFoundException : DomainException
{
    public TaskNotFoundException()
        : base("La tarea no existe.")
    {
    }
}
