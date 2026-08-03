using IdeasGroup.Kanban.Domain.Common;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Domain.Entities;

public class BoardColumn : AuditableEntity
{
    private readonly List<KanbanTask> _tasks = new();

    public Guid BoardId { get; private set; }
    public string Name { get; private set; } = null!;
    public int Order { get; private set; }

    public IReadOnlyCollection<KanbanTask> Tasks => _tasks.AsReadOnly();

    private BoardColumn()
    {
    }

    public static BoardColumn Create(Guid boardId, string name, int order)
    {
        if (boardId == Guid.Empty)
        {
            throw new DomainException("La columna debe pertenecer a un tablero.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre de la columna es obligatorio.");
        }

        return new BoardColumn
        {
            BoardId = boardId,
            Name = name.Trim(),
            Order = order
        };
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre de la columna es obligatorio.");
        }

        Name = name.Trim();
        Touch();
    }

    public void MoveTo(int order)
    {
        Order = order;
        Touch();
    }
}
