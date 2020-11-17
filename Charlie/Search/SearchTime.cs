using System;

namespace Charlie.Search
{
    public record SearchTime(int AvailableTime, int Increment)
    {
        public int MaxTime => AvailableTime / 5;

        public bool CanContinueSearching(long elapsedMs, Score eval)
        {
            var denominator = 120 + Math.Clamp((int)eval, -100, 100);
            return elapsedMs <= AvailableTime / denominator + Increment / 4;
        }
    }
}
