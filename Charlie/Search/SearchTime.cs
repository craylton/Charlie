using System;

namespace Charlie.Search
{
    public record SearchTime(int AvailableTime, int Increment)
    {
        public int MaxTime => Math.Max(AvailableTime / 2, 1);

        public bool CanContinueSearching(
            long elapsedMs,
            Score eval,
            bool bestMoveChanged,
            int bestMoveConfidence)
        {
            // Don't start searching another depth if it's unlikely we'll finish it
            if (elapsedMs > MaxTime / 4)
                return false;

            double timeForMove = AvailableTime / 80;

            // Use more time if we aren't doing well
            if ((int)eval < 50)
            {
                double multiplier = (450 - (int)eval) / 400d;
                timeForMove *= Math.Clamp(multiplier, 1.0, 1.5);
            }

            // Use lower proportion of time when clock is very low
            if (AvailableTime < 10000)
            {
                double multiplier = Math.Sqrt(AvailableTime) / 125 + 0.2;
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
                timeForMove *= 1.8;

            // Use less time if we are very confident about the best move
            timeForMove *= 26 / (Math.Pow(bestMoveConfidence, 2 / 3d) + 17);

            return elapsedMs <= timeForMove + Increment / 4 + 10;
        }
    }
}
