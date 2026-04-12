using System;

namespace Charlie.Search;

public record SearchTime(int AvailableTime, int Increment)
{
    public int MaxTime => Math.Clamp(3 * AvailableTime / 4 + Increment / 2, 1, AvailableTime);

    public bool CanContinueSearching(
        long elapsedMs,
        Score eval,
        bool bestMoveChanged,
        double bestMoveConfidence)
    {
        // Don't start searching another depth if it's unlikely we'll finish it
        long predictedTimeForNextDepth = elapsedMs * 3;
        if ((elapsedMs + predictedTimeForNextDepth) > MaxTime)
            return false;

        double timeForMove = AvailableTime / 25d;

        // Use more time if we aren't doing well
        if ((int)eval < 50)
        {
            double multiplier = (450 - (int)eval) / 400d;
            timeForMove *= Math.Clamp(multiplier, 1.0, 1.5);
        }

        // Use lower proportion of time when clock is very low
        if (AvailableTime < 10000)
        {
            double multiplier = (AvailableTime / 20000d) + 0.5;
            timeForMove *= multiplier;
        }

        // Use more time depending on size of increment
        if (Increment > 0)
        {
            double multiplier = (Increment + 10000) / 10000d;
            timeForMove *= Math.Clamp(multiplier, 1.0, 1.3);
        }

        // Use more time if we just changed our minds about the best move
        if (bestMoveChanged)
            timeForMove *= 180d / 100d;

        // Use less time if we are very confident about the best move
        timeForMove *= 1.5 - Math.Clamp(bestMoveConfidence, 0, 1);

        // Make sure we haven't assigned ourselves more time than we had available
        timeForMove = Math.Min(timeForMove, MaxTime);

        return elapsedMs <= timeForMove;
    }
}
