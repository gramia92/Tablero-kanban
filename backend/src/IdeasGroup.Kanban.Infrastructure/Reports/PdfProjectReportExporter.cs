using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Application.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace IdeasGroup.Kanban.Infrastructure.Reports;

public class PdfProjectReportExporter : IReportExporter
{
    public ReportFormat Format => ReportFormat.Pdf;

    public ExportedFile Export(ProjectReport report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text(report.ProjectName).FontSize(18).Bold();
                    column.Item().Text($"Reporte generado: {report.GeneratedAtUtc:yyyy-MM-dd HH:mm} UTC")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        foreach (var title in new[] { "Columna", "Tarea", "Prioridad", "Responsable", "Vence" })
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(title).Bold();
                        }
                    });

                    foreach (var row in report.Rows)
                    {
                        table.Cell().Padding(4).Text(row.ColumnName);
                        table.Cell().Padding(4).Text(row.Title);
                        table.Cell().Padding(4).Text(row.Priority);
                        table.Cell().Padding(4).Text(row.AssigneeName ?? "-");
                        table.Cell().Padding(4).Text(row.DueDate.HasValue ? row.DueDate.Value.ToString("yyyy-MM-dd") : "-");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        var bytes = document.GeneratePdf();
        var fileName = $"reporte-{ReportFileNaming.Slugify(report.ProjectName)}.pdf";

        return new ExportedFile(bytes, "application/pdf", fileName);
    }
}
