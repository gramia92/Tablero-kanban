using ClosedXML.Excel;
using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Application.Reports;

namespace IdeasGroup.Kanban.Infrastructure.Reports;

public class ExcelProjectReportExporter : IReportExporter
{
    private static readonly string[] Headers = { "Columna", "Tarea", "Descripción", "Prioridad", "Responsable", "Vence", "Creada" };

    public ReportFormat Format => ReportFormat.Excel;

    public ExportedFile Export(ProjectReport report)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Reporte");

        sheet.Cell(1, 1).Value = report.ProjectName;
        sheet.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(14);
        sheet.Cell(2, 1).Value = $"Generado: {report.GeneratedAtUtc:yyyy-MM-dd HH:mm} UTC";

        const int headerRow = 4;
        for (var i = 0; i < Headers.Length; i++)
        {
            var cell = sheet.Cell(headerRow, i + 1);
            cell.Value = Headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        var rowIndex = headerRow + 1;
        foreach (var row in report.Rows)
        {
            sheet.Cell(rowIndex, 1).Value = row.ColumnName;
            sheet.Cell(rowIndex, 2).Value = row.Title;
            sheet.Cell(rowIndex, 3).Value = row.Description ?? string.Empty;
            sheet.Cell(rowIndex, 4).Value = row.Priority;
            sheet.Cell(rowIndex, 5).Value = row.AssigneeName ?? string.Empty;

            if (row.DueDate.HasValue)
            {
                sheet.Cell(rowIndex, 6).Value = row.DueDate.Value;
                sheet.Cell(rowIndex, 6).Style.DateFormat.Format = "yyyy-MM-dd";
            }

            sheet.Cell(rowIndex, 7).Value = row.CreatedAtUtc;
            sheet.Cell(rowIndex, 7).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";

            rowIndex++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var fileName = $"reporte-{ReportFileNaming.Slugify(report.ProjectName)}.xlsx";

        return new ExportedFile(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
