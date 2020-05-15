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

        private static async Task Main(string[] args)
        {
            searcher.BestMoveChanged += Searcher_BestMoveChanged;
            searcher.SearchComplete += Searcher_SearchComplete;

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
                    var searchTime = new SearchTime(0, 0, true);
                    int targetDepth = -1;

                    if (@params.Length >= 3 && @params[1] == "depth")
                        targetDepth = int.Parse(@params[2]);

                    else if (@params.Length >= 5 && @params[1] == "wtime" && @params[3] == "btime")
                    {
                        int whiteTime = int.Parse(@params[2]);
                        int blackTime = int.Parse(@params[4]);

                        int timeAvailable = boardState.ToMove == PieceColour.White ? whiteTime : blackTime;
                        searchTime = new SearchTime(timeAvailable / 30, timeAvailable / 20, false);
                    }

                    await searcher.Start(boardState, searchTime, targetDepth);
                }

                if (@params[0] == "bench")
                {
                    int targetDepth = 3;

                    if (@params.Length >= 2 && @params[1] == "depth")
                        targetDepth = int.Parse(@params[2]);

                    var bench = new Bench();
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
    }
}
