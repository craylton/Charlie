using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Charlie3
{
    class Program
    {
        private static BoardState boardState;
        private static MoveGenerator generator = new MoveGenerator();

        private static void Main(string[] args)
        {
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
                                List<Move> moves = generator.GenerateLegalMoves(boardState).ToList();
                                boardState = boardState.MakeMove(Move.FromString(moves, @params[i]));
                            }
                        }
                    }
                }

                if (input.StartsWith("go"))
                {
                    var searcher = new Search();
                    Task.Run(async () =>
                    {
                        List<Move> moves = generator.GenerateLegalMoves(boardState).ToList();
                        Move bestMove = await searcher.FindBestMove(moves, boardState);

                        Console.WriteLine("bestmove " + bestMove.ToString());
                        File.AppendAllLines("inputs.txt", new[] { "[BEST MOVE]: " + bestMove.ToString() });

                    });
                }
            }
        }
    }
}
