using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Application.Reports;

namespace IdeasGroup.Kanban.Application.Tests.Reports;

public class FakeReportExporter : IReportExporter
{
    public FakeReportExporter(ReportFormat format)
    {
        Format = format;
    }

    public ReportFormat Format { get; }

    public ProjectReport? LastReport { get; private set; }

    public ExportedFile Export(ProjectReport report)
    {
        LastReport = report;
        return new ExportedFile(new byte[] { 1, 2, 3 }, $"application/{Format}", $"reporte.{Format}");
    }
}
