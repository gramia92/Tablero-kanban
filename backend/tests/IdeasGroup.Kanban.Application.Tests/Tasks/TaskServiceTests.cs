using IdeasGroup.Kanban.Application.Boards;
using IdeasGroup.Kanban.Application.Tasks;
using IdeasGroup.Kanban.Application.Tests.Auth;
using IdeasGroup.Kanban.Application.Tests.Boards;
using IdeasGroup.Kanban.Application.Tests.Projects;
using IdeasGroup.Kanban.Domain.Entities;
using IdeasGroup.Kanban.Domain.Enums;
using IdeasGroup.Kanban.Domain.Exceptions;
using IdeasGroup.Kanban.Domain.Services;
using Xunit;

namespace IdeasGroup.Kanban.Application.Tests.Tasks;

public class TaskServiceTests
{
    private static (TaskService Service, Project Project, BoardColumn ColumnA, BoardColumn ColumnB, FakeTaskRepository TaskRepository) BuildScenario()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Proyecto A", null, ownerId);
        var board = Board.Create(project.Id, "Tablero principal");
        var columnA = board.AddColumn("Columna A");
        var columnB = board.AddColumn("Columna B");

        var projectRepository = new FakeProjectRepository().Seed(project);
        var boardRepository = new FakeBoardRepository().Seed(board);
        var taskRepository = new FakeTaskRepository();
        var service = new TaskService(taskRepository, boardRepository, projectRepository, new FakeUserRepository(), new FakeBoardRealtimeNotifier());

        return (service, project, columnA, columnB, taskRepository);
    }

    [Fact]
    public async Task CreateAsync_ubica_la_primera_tarea_en_el_paso_inicial()
    {
        var (service, project, columnA, _, _) = BuildScenario();

        var response = await service.CreateAsync(
            project.OwnerId, project.Id,
            new CreateTaskRequest(columnA.Id, "Primera tarea", null, Priority.Medium, null, null));

        Assert.Equal(TaskPositionCalculator.InitialStep, response.Position);
    }

    [Fact]
    public async Task CreateAsync_agrega_al_final_con_un_paso_de_diferencia()
    {
        var (service, project, columnA, _, taskRepository) = BuildScenario();
        taskRepository.Seed(KanbanTask.Create(columnA.Id, "Existente", null, 500d, Priority.Low, project.OwnerId));

        var response = await service.CreateAsync(
            project.OwnerId, project.Id,
            new CreateTaskRequest(columnA.Id, "Nueva tarea", null, Priority.Medium, null, null));

        Assert.Equal(500d + TaskPositionCalculator.InitialStep, response.Position);
    }

    [Fact]
    public async Task CreateAsync_lanza_ForbiddenProjectAccessException_si_no_es_miembro()
    {
        var (service, project, columnA, _, _) = BuildScenario();

        await Assert.ThrowsAsync<ForbiddenProjectAccessException>(() => service.CreateAsync(
            Guid.NewGuid(), project.Id,
            new CreateTaskRequest(columnA.Id, "Tarea", null, Priority.Medium, null, null)));
    }

    [Fact]
    public async Task CreateAsync_lanza_BoardColumnNotFoundException_si_la_columna_no_es_del_tablero()
    {
        var (service, project, _, _, _) = BuildScenario();

        await Assert.ThrowsAsync<BoardColumnNotFoundException>(() => service.CreateAsync(
            project.OwnerId, project.Id,
            new CreateTaskRequest(Guid.NewGuid(), "Tarea", null, Priority.Medium, null, null)));
    }

    [Fact]
    public async Task UpdateAsync_modifica_los_campos_de_la_tarea()
    {
        var (service, project, columnA, _, taskRepository) = BuildScenario();
        var task = KanbanTask.Create(columnA.Id, "Original", null, 100d, Priority.Low, project.OwnerId);
        taskRepository.Seed(task);

        var response = await service.UpdateAsync(
            project.OwnerId, project.Id, task.Id,
            new UpdateTaskRequest("Actualizada", "Nueva descripción", Priority.High, null, null));

        Assert.Equal("Actualizada", response.Title);
        Assert.Equal(Priority.High, response.Priority);
    }

    [Fact]
    public async Task DeleteAsync_elimina_la_tarea()
    {
        var (service, project, columnA, _, taskRepository) = BuildScenario();
        var task = KanbanTask.Create(columnA.Id, "A borrar", null, 100d, Priority.Low, project.OwnerId);
        taskRepository.Seed(task);

        await service.DeleteAsync(project.OwnerId, project.Id, task.Id);

        Assert.Null(await taskRepository.GetByIdAsync(task.Id));
    }

    [Fact]
    public async Task MoveAsync_calcula_el_punto_medio_entre_dos_tareas_vecinas()
    {
        var (service, project, columnA, columnB, taskRepository) = BuildScenario();
        var before = KanbanTask.Create(columnB.Id, "Antes", null, 100d, Priority.Low, project.OwnerId);
        var after = KanbanTask.Create(columnB.Id, "Después", null, 200d, Priority.Low, project.OwnerId);
        var moving = KanbanTask.Create(columnA.Id, "Moviendo", null, 65536d, Priority.Low, project.OwnerId);
        taskRepository.Seed(before).Seed(after).Seed(moving);

        var response = await service.MoveAsync(
            project.OwnerId, project.Id, moving.Id,
            new MoveTaskRequest(columnB.Id, before.Id, after.Id));

        Assert.Equal(150d, response.Position);
        Assert.Equal(columnB.Id, response.BoardColumnId);
    }

    [Fact]
    public async Task MoveAsync_rebalancea_la_columna_cuando_no_queda_espacio_entre_vecinas()
    {
        var (service, project, columnA, columnB, taskRepository) = BuildScenario();
        var before = KanbanTask.Create(columnB.Id, "Antes", null, 100d, Priority.Low, project.OwnerId);
        var after = KanbanTask.Create(columnB.Id, "Después", null, 100.0000001d, Priority.Low, project.OwnerId);
        var moving = KanbanTask.Create(columnA.Id, "Moviendo", null, 65536d, Priority.Low, project.OwnerId);
        taskRepository.Seed(before).Seed(after).Seed(moving);

        await service.MoveAsync(
            project.OwnerId, project.Id, moving.Id,
            new MoveTaskRequest(columnB.Id, before.Id, after.Id));

        var reordered = (await taskRepository.ListByColumnAsync(columnB.Id)).ToList();
        Assert.Equal(3, reordered.Count);
        Assert.Equal(new[] { before.Id, moving.Id, after.Id }, reordered.Select(t => t.Id));
        Assert.True(reordered[0].Position < reordered[1].Position);
        Assert.True(reordered[1].Position < reordered[2].Position);
    }
}
