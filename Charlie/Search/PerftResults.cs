namespace Charlie.Search
{
    public record PerftResults(ulong PermutationCount, ulong ElapsedMilliseconds)
    {
        public ulong NodesPerSecond => 1000 * PermutationCount / ElapsedMilliseconds;
    }
}