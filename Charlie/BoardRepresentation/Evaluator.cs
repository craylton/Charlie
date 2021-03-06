using System.Numerics;

namespace Charlie.BoardRepresentation;

public class Evaluator
{
    private const int pawn = 100, knight = 320, bishop = 338, rook = 525, queen = 920;

    private readonly int[] pawnPsqt = new[]
    {
        0,  0,  0,  0,  0,  0,  0,  0,
        70, 70, 70, 70, 70, 70, 70, 70,
        15, 15, 25, 40, 40, 25, 15, 15,
        10, 10, 15, 25, 25, 15, 10, 10,
        0,  0,  0,  20, 20,  0, 0,  0,
        5, -5, -10, 0,  0, -10,-5,  5,
        5, 10, 10, -20,-20, 10,10,  5,
        0,  0,  0,  0,  0,  0,  0,  0,
    };

    private readonly int[] knightPsqt = new[]
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

    private readonly int[] bishopPsqt = new[]
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

    private readonly int[] rookPsqt = new[]
    {
         4,  6, 12, 14, 14, 12,  6,  4,
         4, 12, 21, 10, 10, 21, 12,  4,
        -5,  3,  3, 14, 14,  3,  3, -5,
       -13,  0,  5,  2,  2,  5,  0,-13,
        -6,  1, -3,  4,  4, -3,  1, -6,
        -6, -6,  2,  2,  2,  2, -6, -6,
       -13, -8, -1,  5,  5, -1, -8,-13,
       -17,-13, -9, -4, -4, -9,-13,-17,
    };

    private readonly int[] queenPsqt = new[]
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

    private readonly int[] openingQueenPsqt = new[]
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

    private readonly int[] kingPsqt = new[]
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

    private readonly int[] endgameKingPsqt = new[]
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

    public Score Evaluate(BoardState board)
    {
        Score whiteScore = Score.Draw, blackScore = Score.Draw;

        ulong whiteAttacks = 0ul, blackAttacks = 0ul;

        int whiteMaterial = GetWhiteMaterialCount(board);
        int blackMaterial = GetBlackMaterialCount(board);

        whiteScore += whiteMaterial;
        blackScore += blackMaterial;

        bool isOpening = whiteMaterial + blackMaterial >= 6200;
        bool isEndgame = whiteMaterial + blackMaterial <= 3500;

        ulong unoccupiedBb = ~board.Board.Occupied;

        for (int i = 0; i < 64; i++)
        {
            ulong thisSquare = 1ul << i;
            if ((unoccupiedBb & thisSquare) != 0) continue;

            // Assign values to each piece according to its position
            whiteScore += CalculateWhitePsqt(board, isOpening, isEndgame, i, thisSquare);
            blackScore += CalculateBlackPsqt(board, isOpening, isEndgame, i, thisSquare);
        }

        // Lazy eval
        if ((whiteScore - blackScore) > 270 || (whiteScore - blackScore) < -270)
        {
            if (isEndgame)
                CalculatePawnScores(board, ref whiteScore, ref blackScore);

            return (whiteScore - blackScore) * (board.ToMove == PieceColour.White ? 1 : -1);
        }

        for (int i = 0; i < 64; i++)
        {
            ulong thisSquare = 1ul << i;

            if (board.IsUnderAttack(thisSquare, PieceColour.White)) whiteAttacks |= thisSquare;
            if (board.IsUnderAttack(thisSquare, PieceColour.Black)) blackAttacks |= thisSquare;
        }

        ulong whiteTerritory = whiteAttacks & ~blackAttacks;
        ulong blackTerritory = blackAttacks & ~whiteAttacks;

        whiteScore += (BitOperations.PopCount(whiteAttacks) + BitOperations.PopCount(whiteTerritory)) * 2;
        blackScore += (BitOperations.PopCount(blackAttacks) + BitOperations.PopCount(blackTerritory)) * 2;

        whiteScore += BitOperations.PopCount((board.Board.WhitePawn >> 8) & whiteTerritory) * 8;
        blackScore += BitOperations.PopCount((board.Board.BlackPawn << 8) & blackTerritory) * 8;

        // Hanging pieces are less valuable
        if (board.ToMove == PieceColour.Black)
        {
            whiteScore -= BitOperations.PopCount(blackTerritory & board.Board.WhitePawn) * pawn / 3;
            whiteScore -= BitOperations.PopCount(blackTerritory & board.Board.WhiteKnight) * knight / 2;
            whiteScore -= BitOperations.PopCount(blackTerritory & board.Board.WhiteBishop) * bishop / 2;
            whiteScore -= BitOperations.PopCount(blackTerritory & board.Board.WhiteRook) * rook / 3;
            whiteScore -= BitOperations.PopCount(blackTerritory & board.Board.WhiteQueen) * queen / 3;
        }
        else if (board.ToMove == PieceColour.White)
        {
            blackScore -= BitOperations.PopCount(whiteTerritory & board.Board.BlackPawn) * pawn / 3;
            blackScore -= BitOperations.PopCount(whiteTerritory & board.Board.BlackKnight) * knight / 2;
            blackScore -= BitOperations.PopCount(whiteTerritory & board.Board.BlackBishop) * bishop / 2;
            blackScore -= BitOperations.PopCount(whiteTerritory & board.Board.BlackRook) * rook / 3;
            blackScore -= BitOperations.PopCount(whiteTerritory & board.Board.BlackQueen) * queen / 3;
        }

        CalculatePawnScores(board, ref whiteScore, ref blackScore);

        return (whiteScore - blackScore) * (board.ToMove == PieceColour.White ? 1 : -1);
    }

    private static void CalculatePawnScores(BoardState board, ref Score whiteScore, ref Score blackScore)
    {
        bool[] whitePawnsOnFiles = new bool[8];
        bool[] blackPawnsOnFiles = new bool[8];
        for (int i = 0; i < Chessboard.Files.Length; i++)
        {
            whitePawnsOnFiles[i] = (board.Board.WhitePawn & Chessboard.Files[i]) != 0;
            blackPawnsOnFiles[i] = (board.Board.BlackPawn & Chessboard.Files[i]) != 0;
        }

        for (int i = 0; i < Chessboard.Files.Length; i++)
        {
            bool isWhitePawnToLeft = i != 0 && whitePawnsOnFiles[i - 1];
            bool isWhitePawnToRight = i != Chessboard.Files.Length - 1 && whitePawnsOnFiles[i + 1];

            bool isBlackPawnToLeft = i != 0 && blackPawnsOnFiles[i - 1];
            bool isBlackPawnToRight = i != Chessboard.Files.Length - 1 && blackPawnsOnFiles[i + 1];

            if (whitePawnsOnFiles[i])
            {
                // Isolated pawns
                if (!isWhitePawnToLeft && !isWhitePawnToRight) whiteScore -= 10;

                // Doubled pawns
                if (BitOperations.PopCount(board.Board.WhitePawn & Chessboard.Files[i]) > 1) whiteScore -= 30;

                // Passed pawns
                if (!blackPawnsOnFiles[i] && !isBlackPawnToLeft && !isBlackPawnToRight) whiteScore += 28;
            }

            if (blackPawnsOnFiles[i])
            {
                // Isolated pawns
                if (!isBlackPawnToLeft && !isBlackPawnToRight) blackScore -= 10;

                // Doubled pawns
                if (BitOperations.PopCount(board.Board.BlackPawn & Chessboard.Files[i]) > 1) blackScore -= 30;

                // Passed pawns
                if (!whitePawnsOnFiles[i] && !isWhitePawnToLeft && !isWhitePawnToRight) blackScore += 28;
            }
        }
    }

    private static int GetWhiteMaterialCount(BoardState board)
    {
        int whiteMaterial = 0;
        whiteMaterial += BitOperations.PopCount(board.Board.WhitePawn) * pawn;
        whiteMaterial += BitOperations.PopCount(board.Board.WhiteKnight) * knight;
        whiteMaterial += BitOperations.PopCount(board.Board.WhiteBishop) * bishop;
        whiteMaterial += BitOperations.PopCount(board.Board.WhiteRook) * rook;
        whiteMaterial += BitOperations.PopCount(board.Board.WhiteQueen) * queen;
        return whiteMaterial;
    }

    private static int GetBlackMaterialCount(BoardState board)
    {
        int blackMaterial = 0;
        blackMaterial += BitOperations.PopCount(board.Board.BlackPawn) * pawn;
        blackMaterial += BitOperations.PopCount(board.Board.BlackKnight) * knight;
        blackMaterial += BitOperations.PopCount(board.Board.BlackBishop) * bishop;
        blackMaterial += BitOperations.PopCount(board.Board.BlackRook) * rook;
        blackMaterial += BitOperations.PopCount(board.Board.BlackQueen) * queen;
        return blackMaterial;
    }

    private Score CalculateWhitePsqt(BoardState board, bool isOpening, bool isEndgame, int cellIndex, ulong thisSquare)
    {
        Score psqt = Score.Draw;

        if ((board.Board.WhitePawn & thisSquare) != 0) psqt += pawnPsqt[cellIndex] * (isEndgame ? 2 : 1);
        if ((board.Board.WhiteKnight & thisSquare) != 0) psqt += knightPsqt[cellIndex];
        if ((board.Board.WhiteBishop & thisSquare) != 0) psqt += bishopPsqt[cellIndex];
        if ((board.Board.WhiteRook & thisSquare) != 0) psqt += rookPsqt[cellIndex];
        if ((board.Board.WhiteQueen & thisSquare) != 0)
        {
            psqt += isOpening ? openingQueenPsqt[cellIndex] : queenPsqt[cellIndex];
        }
        if ((board.Board.WhiteKing & thisSquare) != 0)
        {
            psqt += isEndgame ? endgameKingPsqt[cellIndex] : kingPsqt[cellIndex];
        }

        return psqt;
    }

    private Score CalculateBlackPsqt(BoardState board, bool isOpening, bool isEndgame, int cellIndex, ulong thisSquare)
    {
        Score psqt = Score.Draw;

        if ((board.Board.BlackPawn & thisSquare) != 0) psqt += pawnPsqt[63 - cellIndex] * (isEndgame ? 2 : 1);
        if ((board.Board.BlackKnight & thisSquare) != 0) psqt += knightPsqt[63 - cellIndex];
        if ((board.Board.BlackBishop & thisSquare) != 0) psqt += bishopPsqt[63 - cellIndex];
        if ((board.Board.BlackRook & thisSquare) != 0) psqt += rookPsqt[63 - cellIndex];
        if ((board.Board.BlackQueen & thisSquare) != 0)
        {
            psqt += isOpening ? openingQueenPsqt[63 - cellIndex] : queenPsqt[63 - cellIndex];
        }
        if ((board.Board.BlackKing & thisSquare) != 0)
        {
            psqt += isEndgame ? endgameKingPsqt[63 - cellIndex] : kingPsqt[63 - cellIndex];
        }

        return psqt;
    }
}
