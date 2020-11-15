using System;

namespace Charlie.Search
{
    public readonly struct SearchTime
    {
        public int AvailableTime { get; }
        public int Increment { get; }
        public int MaxTime => AvailableTime / 5;

        public SearchTime(int availableTime, int increment) =>
            (AvailableTime, Increment) = (availableTime, increment);

        public bool CanContinueSearching(long elapsedMs, Score eval)
        {
            var denominator = 120 + Math.Clamp((int)eval, -100, 100);
            return elapsedMs <= AvailableTime / denominator + Increment / 4;
        }
    }
}
