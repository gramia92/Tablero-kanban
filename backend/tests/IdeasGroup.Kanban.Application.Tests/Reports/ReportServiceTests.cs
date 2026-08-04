using IdeasGroup.Kanban.Application.Reports;
using IdeasGroup.Kanban.Application.Tests.Auth;
using IdeasGroup.Kanban.Application.Tests.Boards;
using IdeasGroup.Kanban.Application.Tests.Projects;
using IdeasGroup.Kanban.Application.Tests.Tasks;
using IdeasGroup.Kanban.Domain.Entities;
using IdeasGroup.Kanban.Domain.Enums;
using IdeasGroup.Kanban.Domain.Exceptions;
using Xunit;

namespace IdeasGroup.Kanban.Application.Tests.Reports;

public class ReportServiceTests
{
    [Fact]
    public async Task ExportProjectReportAsync_arma_las_filas_en_orden_de_tablero_y_usa_el_exportador_del_formato_pedido()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Proyecto Reporte", null, ownerId);
        var board = Board.Create(project.Id, "Tablero");
        var columnA = board.AddColumn("Columna A");
        var columnB = board.AddColumn("Columna B");

        var taskInB = KanbanTask.Create(columnB.Id, "Tarea en B", null, 100d, Priority.Low, ownerId);
        var firstInA = KanbanTask.Create(columnA.Id, "Primera en A", null, 100d, Priority.High, ownerId);
        var secondInA = KanbanTask.Create(columnA.Id, "Segunda en A", null, 200d, Priority.Medium, ownerId);

        var projectRepository = new FakeProjectRepository().Seed(project);
        var boardRepository = new FakeBoardRepository().Seed(board);
        var taskRepository = new FakeTaskRepository().Seed(taskInB).Seed(firstInA).Seed(secondInA);
        var pdfExporter = new FakeReportExporter(ReportFormat.Pdf);
        var excelExporter = new FakeReportExporter(ReportFormat.Excel);

        var service = new ReportService(
            projectRepository, boardRepository, taskRepository, new FakeUserRepository(),
            new[] { pdfExporter, excelExporter });

        var file = await service.ExportProjectReportAsync(ownerId, project.Id, ReportFormat.Excel);

        Assert.Equal("application/Excel", file.ContentType);
        Assert.Null(pdfExporter.LastReport);
        Assert.NotNull(excelExporter.LastReport);
        Assert.Equal(
            new[] { "Primera en A", "Segunda en A", "Tarea en B" },
            excelExporter.LastReport!.Rows.Select(r => r.Title));
    }

    [Fact]
    public async Task ExportProjectReportAsync_lanza_Forbidden_si_no_es_miembro()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Proyecto Reporte", null, ownerId);
        var board = Board.Create(project.Id, "Tablero");

        var service = new ReportService(
            new FakeProjectRepository().Seed(project),
            new FakeBoardRepository().Seed(board),
            new FakeTaskRepository(),
            new FakeUserRepository(),
            new[] { new FakeReportExporter(ReportFormat.Pdf) });

        await Assert.ThrowsAsync<ForbiddenProjectAccessException>(
            () => service.ExportProjectReportAsync(Guid.NewGuid(), project.Id, ReportFormat.Pdf));
    }

    [Fact]
    public async Task ExportProjectReportAsync_lanza_DomainException_si_el_formato_no_tiene_exportador_registrado()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Proyecto Reporte", null, ownerId);
        var board = Board.Create(project.Id, "Tablero");

        var service = new ReportService(
            new FakeProjectRepository().Seed(project),
            new FakeBoardRepository().Seed(board),
            new FakeTaskRepository(),
            new FakeUserRepository(),
            new[] { new FakeReportExporter(ReportFormat.Pdf) });

        await Assert.ThrowsAsync<DomainException>(
            () => service.ExportProjectReportAsync(ownerId, project.Id, ReportFormat.Excel));
    }
}
