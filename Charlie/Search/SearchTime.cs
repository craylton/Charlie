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

            var denominator = 130;

            // Use more time if we aren't doing well
            if ((int)eval < 50)
                denominator += Math.Max((int)eval - 50, -100) / 2;

            // Use lower proportion of time when clock is very low
            if (AvailableTime < 5000)
                denominator += (5000 - AvailableTime) / 100;

            // Use more time depending on size of increment
            if (Increment > 0)
                denominator -= (int)Math.Log10(Increment) * 15;

            return elapsedMs <= AvailableTime / denominator + Increment / 5;
        }
    }
}
