using System;

namespace Charlie.Hash
{
    public static class Zobrist
    {
        public static long[,] Keys { get; private set; } = new long[12, 64];

        public static void Initialise()
        {
            var random = new Random(1255702);

            for (int pieceType = (int)PieceType.WhiteKing; pieceType <= (int)PieceType.BlackPawn; pieceType++)
            {
                for (int cellNumber = 0; cellNumber < 64; cellNumber++)
                {
                    Keys[pieceType, cellNumber] = random.Next(int.MinValue, int.MaxValue);
                }
            }
        }
    }
}
