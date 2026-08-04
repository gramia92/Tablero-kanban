namespace IdeasGroup.Kanban.Application.Boards;

public record ColumnResponse(Guid Id, string Name, int Order, int TaskCount);
