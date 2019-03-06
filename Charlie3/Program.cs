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
            searcher.MoveInfoChanged += Searcher_MoveInfoChanged;

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
                    Task.Run(async () =>
                    {
                        Move bestMove = await searcher.FindBestMove(boardState);
                        sw.Stop();

                        Console.WriteLine("bestmove " + bestMove.ToString());
                        File.AppendAllLines("inputs.txt", new[] { "[BEST MOVE]: " + bestMove.ToString() });
                    });
                }
            }
        }

        private static void Searcher_MoveInfoChanged(object sender, MoveInfo moveInfo)
        {
            var sb = new StringBuilder("info");
            sb.Append(" depth " + moveInfo.Depth);
            sb.Append(" time " + sw.ElapsedMilliseconds);
            sb.Append(" pv " + moveInfo.Moves.FirstOrDefault().ToString());
            sb.Append(" score cp " + moveInfo.Evaluation);

            Console.WriteLine(sb.ToString());
        }
    }
}
