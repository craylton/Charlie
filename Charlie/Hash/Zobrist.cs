using System;

namespace Charlie.Hash
{
    public static class Zobrist
    {
        public static long[,] Keys { get; private set; } = new long[12, 64];

        public static void Initialise()
        {
            var rng = new Random(56810);

            for (int pieceType = (int)PieceType.WhiteKing; pieceType <= (int)PieceType.BlackPawn; pieceType++)
            {
                for (int cellNumber = 0; cellNumber < 64; cellNumber++)
                {
                    Keys[pieceType, cellNumber] = RandomLong(rng);
                }
            }
        }

        private static long RandomLong(Random rng)
        {
            byte[] buf = new byte[8];
            rng.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
        }
    }
}
