using IdeasGroup.Kanban.Domain.Entities;
using IdeasGroup.Kanban.Domain.Enums;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Domain.Tests.Entities;

public class ProjectTests
{
    [Fact]
    public void Create_WithoutDates_DefaultsToPlannedStatus()
    {
        var project = Project.Create("Proyecto A", "Descripción", Guid.NewGuid());

        Assert.Null(project.StartDate);
        Assert.Null(project.ExpectedEndDate);
        Assert.Equal(ProjectStatus.Planned, project.Status);
    }

    [Fact]
    public void Create_WithExpectedEndDateBeforeStartDate_ThrowsDomainException()
    {
        var start = new DateTime(2026, 6, 1);
        var end = new DateTime(2026, 5, 1);

        Assert.Throws<DomainException>(() => Project.Create("Proyecto A", null, Guid.NewGuid(), start, end));
    }

    [Fact]
    public void Update_ChangesDatesAndStatus()
    {
        var project = Project.Create("Proyecto A", null, Guid.NewGuid());
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 12, 31);

        project.Update("Proyecto A renombrado", "Nueva descripción", start, end, ProjectStatus.InProgress);

        Assert.Equal(start, project.StartDate);
        Assert.Equal(end, project.ExpectedEndDate);
        Assert.Equal(ProjectStatus.InProgress, project.Status);
    }

    [Fact]
    public void Update_WithExpectedEndDateBeforeStartDate_ThrowsDomainException()
    {
        var project = Project.Create("Proyecto A", null, Guid.NewGuid());
        var start = new DateTime(2026, 6, 1);
        var end = new DateTime(2026, 5, 1);

        Assert.Throws<DomainException>(() => project.Update("Proyecto A", null, start, end, ProjectStatus.Planned));
    }
}
