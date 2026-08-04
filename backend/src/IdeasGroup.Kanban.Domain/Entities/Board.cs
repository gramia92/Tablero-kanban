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

    public void RenameColumn(Guid columnId, string name)
    {
        var column = GetColumnOrThrow(columnId);
        column.Rename(name);
        Touch();
    }

    public void RemoveColumn(Guid columnId)
    {
        var column = GetColumnOrThrow(columnId);
        if (column.Tasks.Count > 0)
        {
            throw new ColumnHasTasksException();
        }

        _columns.Remove(column);
        Touch();
    }

    public void ReorderColumns(IReadOnlyList<Guid> orderedColumnIds)
    {
        if (orderedColumnIds.Count != _columns.Count || orderedColumnIds.Distinct().Count() != _columns.Count ||
            orderedColumnIds.Any(id => _columns.All(c => c.Id != id)))
        {
            throw new DomainException("El nuevo orden debe incluir exactamente las columnas actuales del tablero.");
        }

        for (var i = 0; i < orderedColumnIds.Count; i++)
        {
            GetColumnOrThrow(orderedColumnIds[i]).MoveTo(i);
        }

        Touch();
    }

    private BoardColumn GetColumnOrThrow(Guid columnId)
    {
        return _columns.FirstOrDefault(c => c.Id == columnId)
            ?? throw new BoardColumnNotFoundException();
    }
}
