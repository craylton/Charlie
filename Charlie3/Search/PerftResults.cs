namespace Charlie.Search
{
    public readonly struct PerftResults
    {
        public ulong PermutationCount { get; }
        public ulong ElapsedMilliseconds { get; }
        public ulong NodesPerSecond => 1000 * PermutationCount / ElapsedMilliseconds;

        public PerftResults(ulong permutationCount, ulong elapsedMilliseconds) =>
            (PermutationCount, ElapsedMilliseconds) = (permutationCount, elapsedMilliseconds);
    }
}