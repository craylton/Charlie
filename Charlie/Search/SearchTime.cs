using System;

namespace Charlie.Search
{
    public record SearchTime(int AvailableTime, int Increment)
    {
        public int MaxTime => Math.Max(AvailableTime / 5, 1);

        public bool CanContinueSearching(long elapsedMs, Score eval)
        {
            var denominator = 120 + Math.Clamp((int)eval, -100, 100);

            // Use lower proportion of time when clock is very low
            if (AvailableTime < 5000)
                denominator -= (AvailableTime - 5000) / 100;

            return elapsedMs <= AvailableTime / denominator + Increment / 4;
        }
    }
}
