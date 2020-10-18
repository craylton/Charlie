namespace Charlie.Search
{
    public readonly struct SearchParameters
    {
        public SearchType SearchType { get; }
        public SearchTime SearchTime { get; }
        public int DepthLimit { get; }

        public SearchParameters(SearchType searchType, SearchTime searchTime, int depthLimit) =>
            (SearchType, SearchTime, DepthLimit) = (searchType, searchTime, depthLimit);

        public bool CanContinueSearching(int nextDepth, long elapsedMs, bool isMate = false)
        {
            if (SearchType == SearchType.Time)
                return !isMate && SearchTime.CanContinueSearching(elapsedMs);

            if (SearchType == SearchType.Depth) 
                return nextDepth <= DepthLimit;

            return true;
        }
    }
}
