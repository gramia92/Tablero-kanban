using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Domain.Services;

public static class TaskPositionCalculator
{
    public const double InitialStep = 65536d;
    private const double MinGap = 1e-6;

    public static double GetPositionForAppend(double? lastPosition)
    {
        return lastPosition.HasValue ? lastPosition.Value + InitialStep : InitialStep;
    }

    public static double CalculatePosition(double? previousPosition, double? nextPosition)
    {
        if (previousPosition is null && nextPosition is null)
        {
            return InitialStep;
        }

        if (previousPosition is null)
        {
            var candidate = nextPosition!.Value / 2;
            if (candidate <= 0 || nextPosition.Value - candidate < MinGap)
            {
                throw new RebalanceRequiredException();
            }

            return candidate;
        }

        if (nextPosition is null)
        {
            return previousPosition.Value + InitialStep;
        }

        if (nextPosition.Value <= previousPosition.Value)
        {
            throw new RebalanceRequiredException();
        }

        var midpoint = previousPosition.Value + (nextPosition.Value - previousPosition.Value) / 2;
        if (midpoint - previousPosition.Value < MinGap || nextPosition.Value - midpoint < MinGap)
        {
            throw new RebalanceRequiredException();
        }

        return midpoint;
    }

    public static IReadOnlyList<double> Rebalance(int itemCount)
    {
        var positions = new double[itemCount];
        for (var i = 0; i < itemCount; i++)
        {
            positions[i] = (i + 1) * InitialStep;
        }

        return positions;
    }
}
