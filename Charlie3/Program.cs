using System;
using System.Linq;

namespace Charlie3
{
    class Program
    {
        static void Main(string[] args)
        {
            var r = new Random();
            var board = new BoardState();
            var gen = new MoveGenerator();

            for (int i = 0; i < 30; i++)
            {
                var moves = gen.GenerateLegalMoves(board).ToList();
                var selectedMove = moves[r.Next(moves.Count)];

                if (moves.Any(m => m.IsEnPassant))
                    selectedMove = moves.First(m => m.IsEnPassant);

                Console.WriteLine(selectedMove.ToString());

                board = board.MakeMove(selectedMove);
            }

            Console.WriteLine("done");


            Console.Read();
        }
    }
}
