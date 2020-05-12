namespace Charlie3
{
    public readonly struct MoveTimeInfo
    {
        public int IdealTime { get; }
        public int MaxTime { get; }
        public bool IsAnalysis { get; }

        public MoveTimeInfo(int idealTime, int maxTime, bool isAnalysis) =>
            (IdealTime, MaxTime, IsAnalysis) = (idealTime, maxTime, isAnalysis);
    }
}
