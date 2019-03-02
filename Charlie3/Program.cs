using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
                                var moves = generator.GenerateLegalMoves(boardState).ToList();
                                boardState = boardState.MakeMove(Move.FromString(moves, @params[i]));
                            }
                        }
                    }
                }

                if (input.StartsWith("go"))
                {
                    var evaluator = new Evaluator();

                    var moves = generator.GenerateLegalMoves(boardState).ToList();
                    var bestMove = evaluator.FindBestMove(moves, boardState);

                    Console.WriteLine("bestmove " + bestMove.ToString());
                }
            }
        }

        private static void MakeEvaluatedMoves(int numMoves)
        {
            var board = new BoardState();
            var gen = new MoveGenerator();
            var evaluator = new Evaluator();

            var pgn = new StringBuilder();

            for (int i = 0; i < numMoves; i++)
            {
                var moves = gen.GenerateLegalMoves(board).ToList();
                var bestMove = evaluator.FindBestMove(moves, board);

                pgn.Append(bestMove.ToString());
                pgn.Append(" ");

                board = board.MakeMove(bestMove);
            }

            Console.WriteLine("PGN:");
            Console.WriteLine(pgn.ToString());
        }

        private static void MakeRandomMoves(int numMoves)
        {
            var r = new Random();
            var board = new BoardState();
            var gen = new MoveGenerator();

            var moveSeq = new List<int>();
            var pgn = new StringBuilder();

            for (int i = 0; i < numMoves; i++)
            {
                var moves = gen.GenerateLegalMoves(board).ToList();
                var moveIndex = r.Next(moves.Count);
                var selectedMove = moves[moveIndex];
                moveSeq.Add(moveIndex);

                if (moves.Any(m => m.IsEnPassant))
                    selectedMove = moves.First(m => m.IsEnPassant);

                Console.WriteLine($"{moveIndex}: {selectedMove.ToString()}");
                pgn.Append(selectedMove.ToString());
                pgn.Append(" ");

                board = board.MakeMove(selectedMove);
            }

            Console.WriteLine("PGN:");
            Console.WriteLine(pgn.ToString());
        }

        private static void MakeMoves(List<int> moveIndices)
        {
            var board = new BoardState();
            var gen = new MoveGenerator();

            foreach (var i in moveIndices)
            {
                var moves = gen.GenerateLegalMoves(board).ToList();
                var selectedMove = moves[i];

                if (moves.Any(m => m.IsEnPassant))
                    selectedMove = moves.First(m => m.IsEnPassant);

                Console.WriteLine(selectedMove.ToString());
                board = board.MakeMove(selectedMove);
            }
        }
    }
}
