namespace Charlie.Search
{
    public readonly struct SearchTime
    {
        public int IdealTime { get; }
        public int MaxTime { get; }
        public bool IsAnalysis { get; }

        public SearchTime(int idealTime, int maxTime, bool isAnalysis) =>
            (IdealTime, MaxTime, IsAnalysis) = (idealTime, maxTime, isAnalysis);
    }
}
