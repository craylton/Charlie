namespace Charlie.Search
{
    public record SearchParameters(SearchType SearchType, SearchTime SearchTime, int DepthLimit)
    {
        public bool CanContinueSearching(
            int nextDepth,
            long elapsedMs,
            Score eval,
            bool bestMoveChanged,
            int bestMoveConfidence,
            bool isMate = false)
        {
            if (SearchType == SearchType.Time)
                return !isMate && SearchTime.CanContinueSearching(
                    elapsedMs,
                    eval,
                    bestMoveChanged,
                    bestMoveConfidence);

            if (SearchType == SearchType.Depth)
                return nextDepth <= DepthLimit;

            return true;
        }
    }
}
