namespace Charlie.Search
{
    public record SearchParameters(SearchType SearchType, SearchTime SearchTime, int DepthLimit)
    {
        public bool CanContinueSearching(int nextDepth, long elapsedMs, Score eval, bool bestMoveChanged, bool isMate = false)
        {
            if (SearchType == SearchType.Time)
                return !isMate && SearchTime.CanContinueSearching(elapsedMs, eval, bestMoveChanged);

            if (SearchType == SearchType.Depth)
                return nextDepth <= DepthLimit;

            return true;
        }
    }
}
