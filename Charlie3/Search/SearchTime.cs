namespace Charlie.Search
{
    public readonly struct SearchTime
    {
        public int IdealTime { get; }
        public int MaxTime { get; }

        public SearchTime(int idealTime, int maxTime) =>
            (IdealTime, MaxTime) = (idealTime, maxTime);
    }
}
