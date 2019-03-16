using Charlie3.Enums;

namespace Charlie3
{
    public class Evaluator
    {
        private const int pawn = 100, knight = 320, bishop = 330, rook = 500, queen = 900, king = 20000;

        private readonly int[] pawnValues = new[]
        {
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5,  10, 25, 25, 10, 5,  5,
            0,  0,  0,  20, 20,  0, 0,  0,
            5, -5, -10, 0,  0, -10,-5,  5,
            5, 10, 10, -20,-20, 10,10,  5,
            0,  0,  0,  0,  0,  0,  0,  0,
        };

        private readonly int[] knightValues = new[]
        {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50,
        };

        private readonly int[] bishopValues = new[]
        {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20,
        };

        private readonly int[] rookValues = new[]
        {
             0,  0,  0,  0,  0,  0,  0,  0,
             5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
             0,  0,  0,  5,  5,  0,  0,  0,
        };

        private readonly int[] queenValues = new[]
        {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
             -5,  0,  5,  5,  5,  5,  0, -5,
              0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20,
        };

        private readonly int[] kingValues = new[]
        {
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
             20, 20,  0,  0,  0,  0, 20, 20,
             20, 30, 10,  0,  0, 10, 30, 20
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

            var unoccupiedBb = ~board.BitBoard.Occupied;

            if (board.IsInPseudoCheck(PieceColour.White)) whiteScore += 25;
            if (board.IsInPseudoCheck(PieceColour.Black)) blackScore += 25;

            for (int i = 0; i < 64; i++)
            {
                var thisSquare = 1ul << i;
                if ((unoccupiedBb & thisSquare) != 0) continue;

                // Assign values to each piece according to its position
                if ((board.BitBoard.WhitePawn & thisSquare) != 0) whiteScore += pawnValues[i];
                if ((board.BitBoard.WhiteKnight & thisSquare) != 0) whiteScore += knightValues[i];
                if ((board.BitBoard.WhiteBishop & thisSquare) != 0) whiteScore += bishopValues[i];
                if ((board.BitBoard.WhiteRook & thisSquare) != 0) whiteScore += rookValues[i];
                if ((board.BitBoard.WhiteQueen & thisSquare) != 0) whiteScore += queenValues[i];
                if ((board.BitBoard.WhiteKing & thisSquare) != 0) whiteScore += kingValues[i];

                if ((board.BitBoard.BlackPawn & thisSquare) != 0) blackScore += pawnValues[63 - i];
                if ((board.BitBoard.BlackKnight & thisSquare) != 0) blackScore += knightValues[63 - i];
                if ((board.BitBoard.BlackBishop & thisSquare) != 0) blackScore += bishopValues[63 - i];
                if ((board.BitBoard.BlackRook & thisSquare) != 0) blackScore += rookValues[63 - i];
                if ((board.BitBoard.BlackQueen & thisSquare) != 0) blackScore += queenValues[63 - i];
                if ((board.BitBoard.BlackKing & thisSquare) != 0) blackScore += kingValues[63 - i];
            }

            for (int i = 0; i < ChessBoard.Files.Length; i++)
            {
                // Check for isolated pawns
                if ((board.BitBoard.WhitePawn & ChessBoard.Files[i]) != 0)
                {
                    bool isPawnToLeft = i != 0 && (board.BitBoard.WhitePawn & ChessBoard.Files[i - 1]) != 0;
                    bool IsPawnToRight = i != ChessBoard.Files.Length - 1 && 
                                        (board.BitBoard.WhitePawn & ChessBoard.Files[i + 1]) != 0;

                    if (!isPawnToLeft && !IsPawnToRight) whiteScore -= 20;
                }

                if ((board.BitBoard.BlackPawn & ChessBoard.Files[i]) != 0)
                {
                    bool isPawnToLeft = i != 0 && (board.BitBoard.BlackPawn & ChessBoard.Files[i - 1]) != 0;
                    bool IsPawnToRight = i != ChessBoard.Files.Length - 1 && 
                                        (board.BitBoard.BlackPawn & ChessBoard.Files[i + 1]) != 0;

                    if (!isPawnToLeft && !IsPawnToRight) blackScore -= 20;
                }

                // Check for doubled pawns
                if ((board.BitBoard.WhitePawn & ChessBoard.Files[i]).BitCount() > 1) whiteScore -= 20;
                if ((board.BitBoard.BlackPawn & ChessBoard.Files[i]).BitCount() > 1) blackScore -= 20;
            }

            return whiteScore - blackScore;
        }
    }
}
