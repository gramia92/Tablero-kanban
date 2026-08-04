namespace IdeasGroup.Kanban.Application.Boards;

public record BoardResponse(Guid Id, Guid ProjectId, string Name, IReadOnlyList<ColumnResponse> Columns);
