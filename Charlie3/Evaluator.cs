namespace Charlie3
{
    public class Evaluator
    {
        private const int pawn = 100, knight = 300, bishop = 310, rook = 500, queen = 900, king = 100000;

        private readonly int[] cellValues = new[]
        {
            -5,-3,0,2,2,0,-3,-5,
            -2,0, 2,3,3,2, 0,-2,
            0, 1, 3,4,4,3, 1, 0,
            2, 4, 5,7,7,5, 4, 2,
            2, 4, 5,7,7,5, 4, 2,
            0, 1, 3,4,4,3, 1, 0,
            -2,0, 2,3,3,2, 0,-2,
            -5,-3,0,2,2,0,-3,-5,
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

            var whitePiecesBb = board.BitBoard.WhitePieces;
            var blackPiecesBb = board.BitBoard.BlackPieces;
            var unoccupiedBb = ~board.BitBoard.Occupied;

            for (int i = 0; i < 64; i++)
            {
                var thisSquare = 1ul << i;
                if ((unoccupiedBb & thisSquare) != 0) continue;

                if ((board.BitBoard.WhiteKing & thisSquare & 0xFF_00_00_00_00_00_00_00) != 0) whiteScore += 10;
                if ((board.BitBoard.BlackKing & thisSquare & 0x00_00_00_00_00_00_00_FF) != 0) blackScore += 10;

                if ((whitePiecesBb & thisSquare) != 0) whiteScore += cellValues[i];
                if ((blackPiecesBb & thisSquare) != 0) blackScore += cellValues[i];
            }

            return whiteScore - blackScore;
        }
    }
}
