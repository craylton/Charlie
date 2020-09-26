namespace Charlie.BenchTest
{
    public readonly struct BenchResults
    {
        public ulong NodesSearched { get; }
        public ulong BenchTimeMs { get; }
        public ulong NodesPerSecond => 1000 * NodesSearched / BenchTimeMs;

        public BenchResults(ulong nodesSearched, ulong benchTimeMs) =>
            (NodesSearched, BenchTimeMs) = (nodesSearched, benchTimeMs);
    }
}
