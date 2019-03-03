namespace Charlie3
{
    public class Evaluator
    {
        private const int pawn = 100, knight = 300, bishop = 310, rook = 500, queen = 900, king = 100000;

        private readonly int[] cellValues = new[]
        {
            -2,-1,0,0,0,0,-1,-2,
            -1,0, 1,1,1,1, 0,-1,
            0, 1, 2,2,2,2, 1, 0,
            1, 2, 3,3,3,3, 2, 1,
            1, 2, 3,3,3,3, 2, 1,
            0, 1, 2,2,2,2, 1, 0,
            -1,0, 1,1,1,1, 0,-1,
            -2,-1,0,0,0,0,-1,-2,
        };

        public int Evaluate(BoardState board)
        {
            int whiteScore = 0, blackScore = 0;

            whiteScore += board.BitBoard.WhitePawn.BitCount() * pawn;
            whiteScore += board.BitBoard.WhiteKnight.BitCount() * knight;
            whiteScore += board.BitBoard.WhiteBishop.BitCount() * bishop;
            whiteScore += board.BitBoard.WhiteRook.BitCount() * rook;
            whiteScore += board.BitBoard.WhiteQueen.BitCount() * queen;
            whiteScore += board.BitBoard.WhiteKing.BitCount() * king;

            blackScore += board.BitBoard.BlackPawn.BitCount() * pawn;
            blackScore += board.BitBoard.BlackKnight.BitCount() * knight;
            blackScore += board.BitBoard.BlackBishop.BitCount() * bishop;
            blackScore += board.BitBoard.BlackRook.BitCount() * rook;
            blackScore += board.BitBoard.BlackQueen.BitCount() * queen;
            blackScore += board.BitBoard.BlackKing.BitCount() * king;

            for (int i = 0; i < 64; i++)
            {
                if ((board.BitBoard.WhitePieces & (1ul << i)) != 0)
                    whiteScore += cellValues[i];

                if ((board.BitBoard.BlackPieces & (1ul << i)) != 0)
                    blackScore += cellValues[i];
            }

            return whiteScore - blackScore;
        }
    }
}
