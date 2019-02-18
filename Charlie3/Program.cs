using System;
using System.Collections.Generic;
using System.Linq;

namespace Charlie3
{
    class Program
    {
        static void Main(string[] args)
        {
            MakeRandomMoves(40);
            //MakeMoves(new List<int> { 6,1,10,12,7,5,13,13,10,1,11,0,9,4,6,10,2,9,0,11,4,13,7,0,});
            Console.WriteLine("done");
            Console.Read();
        }

        private static void MakeRandomMoves(int numMoves)
        {
            var r = new Random();
            var board = new BoardState();
            var gen = new MoveGenerator();

            var moveSeq = new List<int>();

            for (int i = 0; i < numMoves; i++)
            {
                var moves = gen.GenerateLegalMoves(board).ToList();
                var moveIndex = r.Next(moves.Count);
                var selectedMove = moves[moveIndex];
                moveSeq.Add(moveIndex);

                if (moves.Any(m => m.IsEnPassant))
                    selectedMove = moves.First(m => m.IsEnPassant);

                Console.WriteLine($"{moveIndex}: {selectedMove.ToString()}");

                board = board.MakeMove(selectedMove);
            }
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
