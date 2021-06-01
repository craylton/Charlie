using System;

namespace Charlie.Search
{
    public record SearchTime(int AvailableTime, int Increment)
    {
        public int MaxTime => Math.Max(AvailableTime / 2, 1);

        public bool CanContinueSearching(long elapsedMs, Score eval)
        {
            // Don't start searching another depth if it's unlikely we'll finish it
            if (elapsedMs > MaxTime / 4)
                return false;

            double timeForMove = AvailableTime / 100;

            // Use more time if we aren't doing well
            if ((int)eval < 50)
            {
                double multiplier = (450 - (int)eval) / 400d;
                timeForMove *= Math.Clamp(multiplier, 1.0, 1.5);
            }

            // Use lower proportion of time when clock is very low
            if (AvailableTime < 5000)
            {
                double multiplier = (AvailableTime + 5000) / 10000d;
                timeForMove *= multiplier;
            }

            // Use higher proportion of time when we have plenty of time left
            if (AvailableTime > 10000)
            {
                double multiplier = (Math.Sqrt(AvailableTime) + 400) / 500;
                timeForMove *= Math.Clamp(multiplier, 1.0, 1.5);
            }

            // Use more time depending on size of increment
            if (Increment > 0)
            {
                double multiplier = (Increment + 10000) / 10000d;
                timeForMove *= Math.Clamp(multiplier, 1.0, 1.3);
            }

            return elapsedMs <= timeForMove + Increment / 4 + 10;
        }
    }
}
