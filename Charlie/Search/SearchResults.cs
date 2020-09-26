using Charlie.Moves;

namespace Charlie.Search
{
    public readonly struct SearchResults
    {
        public Move BestMove { get; }
        public ulong NodesSearched { get; }
        public long TimeMs { get; }

        public SearchResults(Move bestMove, ulong nodesSearched, long timeMs) =>
            (BestMove, NodesSearched, TimeMs) = (bestMove, nodesSearched, timeMs);
    }
}
