using IdeasGroup.Kanban.Domain.Entities;
using IdeasGroup.Kanban.Domain.Enums;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Domain.Tests.Entities;

public class KanbanTaskTests
{
    [Fact]
    public void Create_WithValidData_SetsInitialState()
    {
        var columnId = Guid.NewGuid();
        var createdById = Guid.NewGuid();

        var task = KanbanTask.Create(columnId, "Diseñar UI", "Detalle", 1024d, Priority.High, createdById);

        Assert.Equal(columnId, task.BoardColumnId);
        Assert.Equal("Diseñar UI", task.Title);
        Assert.Equal(Priority.High, task.Priority);
        Assert.Null(task.AssigneeId);
    }

    [Fact]
    public void Create_WithEmptyTitle_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            KanbanTask.Create(Guid.NewGuid(), "  ", null, 1024d, Priority.Low, Guid.NewGuid()));
    }

    [Fact]
    public void MoveTo_UpdatesColumnAndPosition()
    {
        var task = KanbanTask.Create(Guid.NewGuid(), "Tarea", null, 1024d, Priority.Medium, Guid.NewGuid());
        var newColumnId = Guid.NewGuid();

        task.MoveTo(newColumnId, 2048d);

        Assert.Equal(newColumnId, task.BoardColumnId);
        Assert.Equal(2048d, task.Position);
    }

    [Fact]
    public void AssignLabel_DoesNotDuplicateSameLabel()
    {
        var task = KanbanTask.Create(Guid.NewGuid(), "Tarea", null, 1024d, Priority.Medium, Guid.NewGuid());
        var labelId = Guid.NewGuid();

        task.AssignLabel(labelId);
        task.AssignLabel(labelId);

        Assert.Single(task.TaskLabels);
    }
}
