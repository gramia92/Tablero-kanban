namespace IdeasGroup.Kanban.Application.Boards;

public record ReorderColumnsRequest(IReadOnlyList<Guid> OrderedColumnIds);
