using System;

namespace Charlie3
{
    class Program
    {
        static void Main(string[] args)
        {
            var board = new BoardState();

            var gen = new MoveGenerator();
            var moves = gen.GenerateLegalMoves(board);

            foreach (var move in moves)
                Console.WriteLine(move.ToString());

            Console.WriteLine("done");


            Console.Read();
        }
    }
}
