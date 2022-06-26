using Charlie.BenchTest;
using Charlie.BoardRepresentation;
using Charlie.Hash;
using Charlie.Search;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BenchComparisons
{
    class Program
    {
        private static readonly Searcher searcher = new();
        private static readonly Bench bench = new();

        static async Task Main(string[] args)
        {
            Zobrist.Initialise();
            Magics.Initialise();
            bench.BenchComplete += Bench_BenchComplete;

            var fens = File.ReadAllLines("fens.txt");
            await bench.BenchTest(searcher, fens, 5);
        }

        private static void Bench_BenchComplete(object sender, BenchResults results)
        {
            Console.WriteLine("Bench test complete");
            Console.WriteLine("Nodes searched: " + results.NodesSearched);
            Console.WriteLine("Time (ms): " + results.BenchTimeMs);
            Console.WriteLine("Nodes per second: " + results.NodesPerSecond);
        }
    }
}
