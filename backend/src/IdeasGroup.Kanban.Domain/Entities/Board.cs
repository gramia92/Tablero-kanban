using IdeasGroup.Kanban.Domain.Common;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Domain.Entities;

public class Board : AuditableEntity
{
    private readonly List<BoardColumn> _columns = new();

    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = null!;

    public IReadOnlyCollection<BoardColumn> Columns => _columns.AsReadOnly();

    private Board()
    {
    }

    public static Board Create(Guid projectId, string name)
    {
        if (projectId == Guid.Empty)
        {
            throw new DomainException("El tablero debe pertenecer a un proyecto.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre del tablero es obligatorio.");
        }

        return new Board
        {
            ProjectId = projectId,
            Name = name.Trim()
        };
    }

    public BoardColumn AddColumn(string name)
    {
        var order = _columns.Count == 0 ? 0 : _columns.Max(c => c.Order) + 1;
        var column = BoardColumn.Create(Id, name, order);
        _columns.Add(column);
        Touch();
        return column;
    }
}
