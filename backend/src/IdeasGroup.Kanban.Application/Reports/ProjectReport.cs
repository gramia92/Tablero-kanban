namespace IdeasGroup.Kanban.Application.Reports;

public record ProjectReport(
    string ProjectName,
    DateTime GeneratedAtUtc,
    IReadOnlyList<ProjectReportRow> Rows);

public record ProjectReportRow(
    string ColumnName,
    string Title,
    string? Description,
    string Priority,
    string? AssigneeName,
    DateTime? DueDate,
    DateTime CreatedAtUtc);
