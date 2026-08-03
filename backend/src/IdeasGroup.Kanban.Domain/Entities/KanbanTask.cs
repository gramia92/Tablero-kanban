using IdeasGroup.Kanban.Domain.Common;
using IdeasGroup.Kanban.Domain.Enums;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Domain.Entities;

public class KanbanTask : AuditableEntity
{
    private readonly List<TaskLabel> _taskLabels = new();

    public Guid BoardColumnId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public double Position { get; private set; }
    public Priority Priority { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public Guid CreatedById { get; private set; }
    public DateTime? DueDate { get; private set; }

    public IReadOnlyCollection<TaskLabel> TaskLabels => _taskLabels.AsReadOnly();

    private KanbanTask()
    {
    }

    public static KanbanTask Create(
        Guid boardColumnId,
        string title,
        string? description,
        double position,
        Priority priority,
        Guid createdById,
        Guid? assigneeId = null,
        DateTime? dueDate = null)
    {
        if (boardColumnId == Guid.Empty)
        {
            throw new DomainException("La tarea debe pertenecer a una columna.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("El título de la tarea es obligatorio.");
        }

        if (createdById == Guid.Empty)
        {
            throw new DomainException("La tarea debe registrar quién la creó.");
        }

        return new KanbanTask
        {
            BoardColumnId = boardColumnId,
            Title = title.Trim(),
            Description = description?.Trim(),
            Position = position,
            Priority = priority,
            CreatedById = createdById,
            AssigneeId = assigneeId,
            DueDate = dueDate
        };
    }

    public void MoveTo(Guid boardColumnId, double position)
    {
        if (boardColumnId == Guid.Empty)
        {
            throw new DomainException("La tarea debe pertenecer a una columna.");
        }

        BoardColumnId = boardColumnId;
        Position = position;
        Touch();
    }

    public void Update(string title, string? description, Priority priority, Guid? assigneeId, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("El título de la tarea es obligatorio.");
        }

        Title = title.Trim();
        Description = description?.Trim();
        Priority = priority;
        AssigneeId = assigneeId;
        DueDate = dueDate;
        Touch();
    }

    public void AssignLabel(Guid labelId)
    {
        if (_taskLabels.Any(tl => tl.LabelId == labelId))
        {
            return;
        }

        _taskLabels.Add(TaskLabel.Create(Id, labelId));
        Touch();
    }

    public void RemoveLabel(Guid labelId)
    {
        var link = _taskLabels.FirstOrDefault(tl => tl.LabelId == labelId);
        if (link is not null)
        {
            _taskLabels.Remove(link);
            Touch();
        }
    }
}
