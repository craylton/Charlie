using Charlie3.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Charlie3
{
    public static class Program
    {
        private static BoardState boardState;
        private static MoveGenerator generator = new MoveGenerator();
        private static Search searcher = new Search();

        private static Stopwatch sw;

        private static void Main(string[] args)
        {
            searcher.BestMoveChanged += Searcher_BestMoveChanged;
            searcher.BestMoveFound += Searcher_BestMoveFound;

            while (true)
            {
                var input = Console.ReadLine();

                File.AppendAllLines("inputs.txt", new[] { input });

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

                if (input.StartsWith("position"))
                {
                    var @params = input.Split(' ');
                    if (@params.Length > 1 && @params[1] == "startpos")
                    {
                        boardState = new BoardState();

                        if (@params.Length > 3 && @params[2] == "moves")
                        {
                            for (int i = 3; i < @params.Length; i++)
                            {
                                List<Move> moves = generator.GenerateLegalMoves(boardState);
                                boardState = boardState.MakeMove(Move.FromString(moves, @params[i]));
                            }
                        }
                    }
                }

                if (input.StartsWith("go"))
                {
                    sw = Stopwatch.StartNew();
                    var @params = input.Split(' ');
                    int timeMs = -1;

                    if (@params.Length >= 5)
                    {
                        if (boardState.ToMove == PieceColour.White && @params[1] == "wtime")
                            timeMs = int.Parse(@params[2]) / 30;
                        else if (boardState.ToMove == PieceColour.Black && @params[3] == "btime")
                            timeMs = int.Parse(@params[4]) / 30;
                    }

                    Task.Run(async () => await searcher.Start(boardState, timeMs));
                }
            }
        }

        private static void Searcher_BestMoveFound(object sender, Move bestMove)
        {
            sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds + "ms");
            Console.WriteLine("bestmove " + bestMove.ToString());
            File.AppendAllLines("inputs.txt", new[] { "[BEST MOVE]: " + bestMove.ToString() });
        }

        private static void Searcher_BestMoveChanged(object sender, MoveInfo moveInfo)
        {
            var sb = new StringBuilder("info");
            sb.Append(" depth " + moveInfo.Depth);
            sb.Append(" time " + sw.ElapsedMilliseconds);
            sb.Append(" pv " + string.Join(' ', moveInfo.Moves.Select(mi => mi.ToString())));
            if (moveInfo.IsMate)
                sb.Append(" score mate " + ((moveInfo.Depth + 1) / 2));
            else
                sb.Append(" score cp " + moveInfo.Evaluation);

            Console.WriteLine(sb.ToString());
        }
    }
}
