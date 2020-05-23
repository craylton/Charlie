namespace Charlie.Search
{
    public readonly struct SearchTime
    {
        public int IdealTime { get; }
        public int MaxTime { get; }
        public int Increment { get; }

        public SearchTime(int idealTime, int maxTime, int increment) =>
            (IdealTime, MaxTime, Increment) = (idealTime, maxTime, increment);
    }
}
