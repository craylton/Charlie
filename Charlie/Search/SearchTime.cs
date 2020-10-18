namespace Charlie.Search
{
    public readonly struct SearchTime
    {
        public int AvailableTime { get; }
        public int Increment { get; }
        public int IdealTime => AvailableTime / 160;
        public int MaxTime => AvailableTime / 5;

        public SearchTime(int availableTime, int increment) =>
            (AvailableTime, Increment) = (availableTime, increment);

        public bool CanContinueSearching(long elapsedMs) =>
            elapsedMs <= IdealTime + Increment / 4;
    }
}
