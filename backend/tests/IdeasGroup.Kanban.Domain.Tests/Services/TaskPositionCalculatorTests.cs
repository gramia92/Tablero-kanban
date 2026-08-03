using IdeasGroup.Kanban.Domain.Exceptions;
using IdeasGroup.Kanban.Domain.Services;

namespace IdeasGroup.Kanban.Domain.Tests.Services;

public class TaskPositionCalculatorTests
{
    [Fact]
    public void CalculatePosition_WhenColumnIsEmpty_ReturnsInitialStep()
    {
        var position = TaskPositionCalculator.CalculatePosition(null, null);

        Assert.Equal(TaskPositionCalculator.InitialStep, position);
    }

    [Fact]
    public void CalculatePosition_WhenMovedToTop_ReturnsHalfOfNext()
    {
        var position = TaskPositionCalculator.CalculatePosition(null, 100d);

        Assert.Equal(50d, position);
    }

    [Fact]
    public void CalculatePosition_WhenMovedToBottom_AddsStepToPrevious()
    {
        var position = TaskPositionCalculator.CalculatePosition(100d, null);

        Assert.Equal(100d + TaskPositionCalculator.InitialStep, position);
    }

    [Fact]
    public void CalculatePosition_WhenMovedBetweenTwoTasks_ReturnsMidpoint()
    {
        var position = TaskPositionCalculator.CalculatePosition(100d, 200d);

        Assert.Equal(150d, position);
    }

    [Fact]
    public void CalculatePosition_WhenNoGapLeftBetweenNeighbors_ThrowsRebalanceRequired()
    {
        Assert.Throws<RebalanceRequiredException>(() => TaskPositionCalculator.CalculatePosition(100d, 100.0000001d));
    }

    [Fact]
    public void CalculatePosition_WhenNextIsBeforePrevious_ThrowsRebalanceRequired()
    {
        Assert.Throws<RebalanceRequiredException>(() => TaskPositionCalculator.CalculatePosition(200d, 100d));
    }

    [Fact]
    public void GetPositionForAppend_WhenColumnIsEmpty_ReturnsInitialStep()
    {
        var position = TaskPositionCalculator.GetPositionForAppend(null);

        Assert.Equal(TaskPositionCalculator.InitialStep, position);
    }

    [Fact]
    public void GetPositionForAppend_WhenColumnHasTasks_AddsStepToLast()
    {
        var position = TaskPositionCalculator.GetPositionForAppend(500d);

        Assert.Equal(500d + TaskPositionCalculator.InitialStep, position);
    }

    [Fact]
    public void Rebalance_ReturnsEvenlySpacedPositionsForRequestedCount()
    {
        var positions = TaskPositionCalculator.Rebalance(3);

        Assert.Equal(new[]
        {
            TaskPositionCalculator.InitialStep,
            TaskPositionCalculator.InitialStep * 2,
            TaskPositionCalculator.InitialStep * 3
        }, positions);
    }
}
