using IdeasGroup.Kanban.Application.Reports;

namespace IdeasGroup.Kanban.Application.Abstractions;

/// <summary>
/// Un exportador por formato. Agregar un formato nuevo (ej. CSV) es implementar esta
/// interfaz y registrarla en el DI — no toca <see cref="ReportFormat"/> ni el servicio
/// que arma el <see cref="ProjectReport"/>, que es el mismo para todos los formatos.
/// </summary>
public interface IReportExporter
{
    ReportFormat Format { get; }

    ExportedFile Export(ProjectReport report);
}
