using System;

namespace Charlie.Search
{
    public record SearchTime(int AvailableTime, int Increment)
    {
        public int MaxTime => Math.Max(AvailableTime / 3, 1);

        public bool CanContinueSearching(long elapsedMs, Score eval)
        {
            // Don't start searching another depth if it's unlikely we'll finish it
            if (elapsedMs > MaxTime / 3)
                return false;

            var denominator = 130;

            // Use more time if we aren't doing well
            if ((int)eval < 50)
                denominator += Math.Max((int)eval - 50, -100) / 2;

            // Use lower proportion of time when clock is very low
            if (AvailableTime < 5000)
                denominator -= (AvailableTime - 5000) / 100;

            return elapsedMs <= AvailableTime / denominator + Increment / 5;
        }
    }
}
