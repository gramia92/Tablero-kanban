namespace IdeasGroup.Kanban.Application.Reports;

public record ExportedFile(byte[] Content, string ContentType, string FileName);
