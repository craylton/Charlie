namespace Charlie3
{
    public class Evaluator
    {
        private const int pawn = 1, knight = 3, bishop = 3, rook = 5, queen = 9, king = 100000;

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

            return whiteScore - blackScore;
        }
    }
}
