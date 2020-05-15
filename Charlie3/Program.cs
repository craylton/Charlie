using Charlie.Board;
using Charlie.Moves;
using Charlie.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Charlie
{
    public static class Program
    {
        private static BoardState boardState;
        private static readonly MoveGenerator generator = new MoveGenerator();
        private static readonly Searcher searcher = new Searcher();
        private static readonly Bench bench = new Bench();

        private static async Task Main(string[] args)
        {
            searcher.BestMoveChanged += Searcher_BestMoveChanged;
            searcher.SearchComplete += Searcher_SearchComplete;
            bench.BenchComplete += Bench_BenchComplete;

            while (true)
            {
                string input = Console.ReadLine();
                File.AppendAllLines("inputs.txt", new[] { input });
                string[] @params = input.Split(' ');

                switch (input)
                {
                    case "uci":
                        Console.WriteLine("id name Charlie");
                        Console.WriteLine("id author Craylton");
                        Console.WriteLine("uciok");
                        break;
                    case "isready":
                        Console.WriteLine("readyok");
                        break;
                    case "stop":
                        searcher.Stop();
                        break;
                    case "quit":
                        return;
                }

                if (@params[0] == "position")
                {
                    var movesIndicatorIndex = 0;

                    if (@params.Length > 1 && @params[1] == "startpos")
                    {
                        boardState = new BoardState();
                        movesIndicatorIndex = 2;
                    }
                    else if (@params.Length >= 8 && @params[1] == "fen")
                    {
                        boardState = new BoardState(@params[2..8]);
                        movesIndicatorIndex = 8;
                    }

                    if (@params.Length > movesIndicatorIndex + 1 && @params[movesIndicatorIndex] == "moves")
                    {
                        foreach (string moveInput in @params[(movesIndicatorIndex + 1)..])
                        {
                            List<Move> moves = generator.GenerateLegalMoves(boardState);
                            Move move = Move.FromString(moves, moveInput);
                            boardState = boardState.MakeMove(move);
                        }
                    }
                }

                if (@params[0] == "go")
                {
                    SearchTime searchTime = default;
                    int targetDepth = default;
                    var searchType = SearchType.Infinite;

                    if (@params.Length >= 3 && @params[1] == "depth")
                    {
                        searchType = SearchType.Depth;
                        targetDepth = int.Parse(@params[2]);
                    }

                    else if (@params.Length >= 5 && @params[1] == "wtime" && @params[3] == "btime")
                    {
                        searchType = SearchType.Time;
                        int whiteTime = int.Parse(@params[2]);
                        int blackTime = int.Parse(@params[4]);

                        int timeAvailable = boardState.ToMove == PieceColour.White ? whiteTime : blackTime;
                        searchTime = new SearchTime(timeAvailable / 30, timeAvailable / 20);
                    }

                    var searchParameters = new SearchParameters(searchType, searchTime, targetDepth);
                    _ = Task.Run(async () => await searcher.Start(boardState, searchParameters));
                }

                if (@params[0] == "bench")
                {
                    int targetDepth = 3;

                    if (@params.Length >= 2 && @params[1] == "depth")
                        targetDepth = int.Parse(@params[2]);

                    await bench.BenchTest(searcher, targetDepth);
                }
            }
        }

        private static void Searcher_SearchComplete(object sender, SearchResults results)
        {
            Console.WriteLine("bestmove " + results.BestMove.ToString());
            File.AppendAllLines("inputs.txt", new[] { "[BEST MOVE]: " + results.BestMove.ToString() });
        }

        private static void Searcher_BestMoveChanged(object sender, MoveInfo moveInfo)
        {
            var sb = new StringBuilder("info");
            sb.Append(" depth " + moveInfo.Depth);
            sb.Append(" time " + moveInfo.Time);
            sb.Append(" nodes " + moveInfo.Nodes);
            sb.Append(" pv " + string.Join(' ', moveInfo.Moves.Select(mi => mi.ToString())));
            if (moveInfo.IsMate)
                sb.Append(" score mate " + (moveInfo.Evaluation < 0 ? "-" : "") + ((moveInfo.Depth + 1) / 2));
            else
                sb.Append(" score cp " + moveInfo.Evaluation);

            Console.WriteLine(sb.ToString());
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
