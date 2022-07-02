namespace Charlie.BenchTest;

public record BenchResults(ulong NodesSearched, ulong BenchTimeMs)
{
    public ulong NodesPerSecond => 1000 * NodesSearched / BenchTimeMs;
}
