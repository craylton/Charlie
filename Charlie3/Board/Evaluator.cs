using System.Numerics;

namespace Charlie.Board
{
    public class Evaluator
    {
        private const int pawn = 100, knight = 320, bishop = 330, rook = 500, queen = 900;

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

        private readonly int[] openingQueenValues = new[]
        {
            -20,-20,-20,-20,-20,-20,-20,-20,
            -20,-20,-20,-20,-20,-20,-20,-20,
            -30,-30,-30,-30,-30,-30,-30,-30,
            -33,-30,-30,-30,-30,-30,-30,-33,
            -29,-24,-19,-16,-16,-19,-24,-29,
             -5, -3,  0,  1,  1,  0, -3, -5,
              0,  2,  5,  7,  7,  5,  2,  0,
              2,  5, 11, 17, 17, 11,  5,  2,
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

        private readonly int[] endgameKingValues = new[]
        {
            -10,-5,  0,  5,  5,  0,-5, -10,
            -5,  0,  5,  8,  8,  5, 0, -5,
             0,  5,  8, 10, 10,  8, 5,  0,
             4,  7, 11, 14, 14, 11, 7,  4,
             4,  7, 11, 14, 14, 11, 7,  4,
             0,  5,  8, 10, 10,  8, 5,  0,
            -5,  0,  5,  8,  8,  5, 0, -5,
            -10,-5,  0,  5,  5,  0,-5, -10,
        };

        public int Evaluate(BoardState board)
        {
            int whiteScore = 0, blackScore = 0;

            ulong whiteAttacks = 0ul, blackAttacks = 0ul;
            int whiteMaterial = 0, blackMaterial = 0;

            whiteMaterial += BitOperations.PopCount(board.BitBoard.WhitePawn) * pawn;
            whiteMaterial += BitOperations.PopCount(board.BitBoard.WhiteKnight) * knight;
            whiteMaterial += BitOperations.PopCount(board.BitBoard.WhiteBishop) * bishop;
            whiteMaterial += BitOperations.PopCount(board.BitBoard.WhiteRook) * rook;
            whiteMaterial += BitOperations.PopCount(board.BitBoard.WhiteQueen) * queen;

            blackMaterial += BitOperations.PopCount(board.BitBoard.BlackPawn) * pawn;
            blackMaterial += BitOperations.PopCount(board.BitBoard.BlackKnight) * knight;
            blackMaterial += BitOperations.PopCount(board.BitBoard.BlackBishop) * bishop;
            blackMaterial += BitOperations.PopCount(board.BitBoard.BlackRook) * rook;
            blackMaterial += BitOperations.PopCount(board.BitBoard.BlackQueen) * queen;

            whiteScore += whiteMaterial;
            blackScore += blackMaterial;

            bool isOpening = whiteMaterial + blackMaterial >= 6200;
            bool isEndgame = whiteMaterial + blackMaterial <= 3600;

            ulong unoccupiedBb = ~board.BitBoard.Occupied;

            if (board.IsInPseudoCheck(PieceColour.White)) whiteScore += 25;
            if (board.IsInPseudoCheck(PieceColour.Black)) blackScore += 25;

            for (int i = 0; i < 64; i++)
            {
                ulong thisSquare = 1ul << i;

                if (board.IsUnderAttack(thisSquare, PieceColour.White)) whiteAttacks |= thisSquare;
                if (board.IsUnderAttack(thisSquare, PieceColour.Black)) blackAttacks |= thisSquare;

                if ((unoccupiedBb & thisSquare) != 0) continue;

                // Assign values to each piece according to its position
                if ((board.BitBoard.WhitePawn & thisSquare) != 0) whiteScore += pawnValues[i];
                if ((board.BitBoard.WhiteKnight & thisSquare) != 0) whiteScore += knightValues[i];
                if ((board.BitBoard.WhiteBishop & thisSquare) != 0) whiteScore += bishopValues[i];
                if ((board.BitBoard.WhiteRook & thisSquare) != 0) whiteScore += rookValues[i];
                if ((board.BitBoard.WhiteQueen & thisSquare) != 0)
                {
                    whiteScore += isOpening ? openingQueenValues[i] : queenValues[i];
                }
                if ((board.BitBoard.WhiteKing & thisSquare) != 0)
                {
                    whiteScore += isEndgame ? endgameKingValues[i] : kingValues[i];
                }

                if ((board.BitBoard.BlackPawn & thisSquare) != 0) blackScore += pawnValues[63 - i];
                if ((board.BitBoard.BlackKnight & thisSquare) != 0) blackScore += knightValues[63 - i];
                if ((board.BitBoard.BlackBishop & thisSquare) != 0) blackScore += bishopValues[63 - i];
                if ((board.BitBoard.BlackRook & thisSquare) != 0) blackScore += rookValues[63 - i];
                if ((board.BitBoard.BlackQueen & thisSquare) != 0)
                {
                    blackScore += isOpening ? openingQueenValues[63 - i] : queenValues[63 - i];
                }
                if ((board.BitBoard.BlackKing & thisSquare) != 0)
                {
                    blackScore += isEndgame ? endgameKingValues[63 - i] : kingValues[63 - i];
                }
            }

            ulong whiteTerritory = whiteAttacks & ~blackAttacks;
            ulong blackTerritory = blackAttacks & ~whiteAttacks;

            whiteScore += BitOperations.PopCount(whiteAttacks) * 5 + BitOperations.PopCount(whiteTerritory) * 5;
            blackScore += BitOperations.PopCount(blackAttacks) * 5 + BitOperations.PopCount(blackTerritory) * 5;

            // Hanging pieces are worth half value
            if (board.ToMove == PieceColour.Black)
            {
                whiteScore -= BitOperations.PopCount(blackTerritory & board.BitBoard.WhitePawn) * pawn / 2;
                whiteScore -= BitOperations.PopCount(blackTerritory & board.BitBoard.WhiteKnight) * knight / 2;
                whiteScore -= BitOperations.PopCount(blackTerritory & board.BitBoard.WhiteBishop) * bishop / 2;
                whiteScore -= BitOperations.PopCount(blackTerritory & board.BitBoard.WhiteRook) * rook / 2;
                whiteScore -= BitOperations.PopCount(blackTerritory & board.BitBoard.WhiteQueen) * queen / 2;
            }
            else if (board.ToMove == PieceColour.White)
            {
                blackScore -= BitOperations.PopCount(whiteTerritory & board.BitBoard.BlackPawn) * pawn / 2;
                blackScore -= BitOperations.PopCount(whiteTerritory & board.BitBoard.BlackKnight) * knight / 2;
                blackScore -= BitOperations.PopCount(whiteTerritory & board.BitBoard.BlackBishop) * bishop / 2;
                blackScore -= BitOperations.PopCount(whiteTerritory & board.BitBoard.BlackRook) * rook / 2;
                blackScore -= BitOperations.PopCount(whiteTerritory & board.BitBoard.BlackQueen) * queen / 2;
            }

            for (int i = 0; i < Chessboard.Files.Length; i++)
            {
                // Check for isolated pawns
                if ((board.BitBoard.WhitePawn & Chessboard.Files[i]) != 0)
                {
                    bool isPawnToLeft = i != 0 && (board.BitBoard.WhitePawn & Chessboard.Files[i - 1]) != 0;
                    bool IsPawnToRight = i != Chessboard.Files.Length - 1 &&
                                        (board.BitBoard.WhitePawn & Chessboard.Files[i + 1]) != 0;

                    if (!isPawnToLeft && !IsPawnToRight) whiteScore -= 20;
                }

                if ((board.BitBoard.BlackPawn & Chessboard.Files[i]) != 0)
                {
                    bool isPawnToLeft = i != 0 && (board.BitBoard.BlackPawn & Chessboard.Files[i - 1]) != 0;
                    bool IsPawnToRight = i != Chessboard.Files.Length - 1 &&
                                        (board.BitBoard.BlackPawn & Chessboard.Files[i + 1]) != 0;

                    if (!isPawnToLeft && !IsPawnToRight) blackScore -= 20;
                }

                // Check for doubled pawns
                if (BitOperations.PopCount(board.BitBoard.WhitePawn & Chessboard.Files[i]) > 1) whiteScore -= 20;
                if (BitOperations.PopCount(board.BitBoard.BlackPawn & Chessboard.Files[i]) > 1) blackScore -= 20;
            }

            return (whiteScore - blackScore) * (board.ToMove == PieceColour.White ? 1 : -1);
        }
    }
}
