using IdeasGroup.Kanban.Application.Boards;
using IdeasGroup.Kanban.Application.Tests.Projects;
using IdeasGroup.Kanban.Domain.Entities;
using IdeasGroup.Kanban.Domain.Exceptions;
using Xunit;

namespace IdeasGroup.Kanban.Application.Tests.Boards;

public class BoardServiceTests
{
    private static (BoardService Service, Project Project, Board Board) BuildScenario()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Proyecto A", null, ownerId);
        var board = Board.Create(project.Id, "Tablero principal");
        board.AddColumn("Por hacer");
        board.AddColumn("En progreso");
        board.AddColumn("Hecho");

        var projectRepository = new FakeProjectRepository().Seed(project);
        var boardRepository = new FakeBoardRepository().Seed(board);
        var service = new BoardService(boardRepository, projectRepository, new FakeBoardRealtimeNotifier());

        return (service, project, board);
    }

    [Fact]
    public async Task AddColumnAsync_permite_agregar_columna_a_un_miembro_del_proyecto()
    {
        var (service, project, _) = BuildScenario();

        var response = await service.AddColumnAsync(project.OwnerId, project.Id, new CreateColumnRequest("Revisión"));

        Assert.Equal("Revisión", response.Name);
        Assert.Equal(3, response.Order);
    }

    [Fact]
    public async Task AddColumnAsync_lanza_ForbiddenProjectAccessException_si_no_es_miembro()
    {
        var (service, project, _) = BuildScenario();

        await Assert.ThrowsAsync<ForbiddenProjectAccessException>(
            () => service.AddColumnAsync(Guid.NewGuid(), project.Id, new CreateColumnRequest("Revisión")));
    }

    [Fact]
    public async Task RenameColumnAsync_actualiza_el_nombre_de_la_columna()
    {
        var (service, project, board) = BuildScenario();
        var columnId = board.Columns.First().Id;

        var response = await service.RenameColumnAsync(project.OwnerId, project.Id, columnId, new RenameColumnRequest("Backlog"));

        Assert.Equal("Backlog", response.Columns.Single(c => c.Id == columnId).Name);
    }

    [Fact]
    public async Task DeleteColumnAsync_elimina_una_columna_sin_tareas()
    {
        var (service, project, board) = BuildScenario();
        var columnId = board.Columns.Last().Id;

        var response = await service.DeleteColumnAsync(project.OwnerId, project.Id, columnId);

        Assert.DoesNotContain(response.Columns, c => c.Id == columnId);
        Assert.Equal(2, response.Columns.Count);
    }

    [Fact]
    public async Task ReorderColumnsAsync_aplica_el_nuevo_orden()
    {
        var (service, project, board) = BuildScenario();
        var newOrder = board.Columns.Select(c => c.Id).Reverse().ToList();

        var response = await service.ReorderColumnsAsync(project.OwnerId, project.Id, new ReorderColumnsRequest(newOrder));

        Assert.Equal(newOrder, response.Columns.OrderBy(c => c.Order).Select(c => c.Id).ToList());
    }
}
