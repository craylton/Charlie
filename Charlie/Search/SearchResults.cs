using Charlie.Moves;

namespace Charlie.Search
{
    public record SearchResults(Move BestMove, ulong NodesSearched, long TimeMs);
}
