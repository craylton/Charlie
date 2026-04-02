using System.Numerics;

namespace Charlie.BoardRepresentation;

public class Evaluator
{
    private const int pawn = 100, knight = 320, bishop = 338, rook = 525, queen = 920;

    private static readonly int[] pawnPsqt =
    [
        0,  0,  0,  0,  0,  0,  0,  0,
        70, 70, 70, 70, 70, 70, 70, 70,
        15, 15, 25, 40, 40, 25, 15, 15,
        10, 10, 15, 25, 25, 15, 10, 10,
        0,  0,  0,  20, 20,  0, 0,  0,
        5, -5, -10, 0,  0, -10,-5,  5,
        5, 10, 10, -20,-20, 10,10,  5,
        0,  0,  0,  0,  0,  0,  0,  0,
    ];

    private static readonly int[] knightPsqt =
    [
        -50,-40,-30,-30,-30,-30,-40,-50,
        -40,-20,  0,  0,  0,  0,-20,-40,
        -30,  0, 10, 15, 15, 10,  0,-30,
        -30,  5, 15, 20, 20, 15,  5,-30,
        -30,  0, 15, 20, 20, 15,  0,-30,
        -30,  5, 10, 15, 15, 10,  5,-30,
        -40,-20,  0,  5,  5,  0,-20,-40,
        -50,-40,-30,-30,-30,-30,-40,-50,
    ];

    private static readonly int[] bishopPsqt =
    [
        -20,-10,-10,-10,-10,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5, 10, 10,  5,  0,-10,
        -10,  5,  5, 10, 10,  5,  5,-10,
        -10,  0, 10, 10, 10, 10,  0,-10,
        -10, 10, 10, 10, 10, 10, 10,-10,
        -10,  5,  0,  0,  0,  0,  5,-10,
        -20,-10,-10,-10,-10,-10,-10,-20,
    ];

    private static readonly int[] rookPsqt =
    [
         4,  6, 12, 14, 14, 12,  6,  4,
         4, 12, 21, 10, 10, 21, 12,  4,
        -5,  3,  3, 14, 14,  3,  3, -5,
       -13,  0,  5,  2,  2,  5,  0,-13,
        -6,  1, -3,  4,  4, -3,  1, -6,
        -6, -6,  2,  2,  2,  2, -6, -6,
       -13, -8, -1,  5,  5, -1, -8,-13,
       -17,-13, -9, -4, -4, -9,-13,-17,
    ];

    private static readonly int[] queenPsqt =
    [
        -20,-10,-10, -5, -5,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5,  5,  5,  5,  0,-10,
         -5,  0,  5,  5,  5,  5,  0, -5,
          0,  0,  5,  5,  5,  5,  0, -5,
        -10,  5,  5,  5,  5,  5,  0,-10,
        -10,  0,  5,  0,  0,  0,  0,-10,
        -20,-10,-10, -5, -5,-10,-10,-20,
    ];

    private static readonly int[] openingQueenPsqt =
    [
        -20,-20,-20,-20,-20,-20,-20,-20,
        -20,-20,-20,-20,-20,-20,-20,-20,
        -30,-30,-30,-30,-30,-30,-30,-30,
        -33,-30,-30,-30,-30,-30,-30,-33,
        -29,-24,-19,-16,-16,-19,-24,-29,
         -5, -3,  0,  1,  1,  0, -3, -5,
          0,  2,  5,  7,  7,  5,  2,  0,
          2,  5, 11, 17, 17, 11,  5,  2,
    ];

    private static readonly int[] kingPsqt =
    [
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -20,-30,-30,-40,-40,-30,-30,-20,
        -10,-20,-20,-20,-20,-20,-20,-10,
         20, 20,  0,  0,  0,  0, 20, 20,
         20, 30, 10,  0,  0, 10, 30, 20
    ];

    private static readonly int[] endgameKingPsqt =
    [
        -10,-5,  0,  5,  5,  0,-5, -10,
        -5,  0,  5,  8,  8,  5, 0, -5,
         0,  5,  8, 10, 10,  8, 5,  0,
         4,  7, 11, 14, 14, 11, 7,  4,
         4,  7, 11, 14, 14, 11, 7,  4,
         0,  5,  8, 10, 10,  8, 5,  0,
        -5,  0,  5,  8,  8,  5, 0, -5,
        -10,-5,  0,  5,  5,  0,-5, -10,
    ];

    private static readonly byte[] adjacentFilesMasks =
    [
        0b0000_0011,
        0b0000_0111,
        0b0000_1110,
        0b0001_1100,
        0b0011_1000,
        0b0111_0000,
        0b1110_0000,
        0b1100_0000,
    ];

    public static Score Evaluate(IPositionState board)
    {
        Board position = board.Board;

        int whiteMaterial = GetWhiteMaterialCount(position);
        int blackMaterial = GetBlackMaterialCount(position);
        int whiteScore = whiteMaterial;
        int blackScore = blackMaterial;
        int totalMaterial = whiteMaterial + blackMaterial;

        bool isOpening = totalMaterial >= 6200;
        bool isEndgame = totalMaterial <= 3500;
        int pawnMultiplier = isEndgame ? 2 : 1;
        int[] queenTable = isOpening ? openingQueenPsqt : queenPsqt;
        int[] kingTable = isEndgame ? endgameKingPsqt : kingPsqt;

        whiteScore += GetPsqtScore(position.WhitePawn, pawnPsqt, pawnMultiplier);
        whiteScore += GetPsqtScore(position.WhiteKnight, knightPsqt);
        whiteScore += GetPsqtScore(position.WhiteBishop, bishopPsqt);
        whiteScore += GetPsqtScore(position.WhiteRook, rookPsqt);
        whiteScore += GetPsqtScore(position.WhiteQueen, queenTable);
        whiteScore += GetPsqtScore(position.WhiteKing, kingTable);

        blackScore += GetMirroredPsqtScore(position.BlackPawn, pawnPsqt, pawnMultiplier);
        blackScore += GetMirroredPsqtScore(position.BlackKnight, knightPsqt);
        blackScore += GetMirroredPsqtScore(position.BlackBishop, bishopPsqt);
        blackScore += GetMirroredPsqtScore(position.BlackRook, rookPsqt);
        blackScore += GetMirroredPsqtScore(position.BlackQueen, queenTable);
        blackScore += GetMirroredPsqtScore(position.BlackKing, kingTable);

        // Lazy eval
        int score = whiteScore - blackScore;
        if (score > 270 || score < -270)
        {
            if (isEndgame)
                CalculatePawnScores(position, isEndgame, ref whiteScore, ref blackScore);

            score = whiteScore - blackScore;
            return new Score(score * (board.ToMove == PieceColour.White ? 1 : -1));
        }

        ulong whiteAttacks = GetWhiteAttacks(position);
        ulong blackAttacks = GetBlackAttacks(position);

        ulong whiteTerritory = whiteAttacks & ~blackAttacks;
        ulong blackTerritory = blackAttacks & ~whiteAttacks;

        whiteScore += (BitOperations.PopCount(whiteAttacks) + BitOperations.PopCount(whiteTerritory)) * 2;
        blackScore += (BitOperations.PopCount(blackAttacks) + BitOperations.PopCount(blackTerritory)) * 2;

        whiteScore += BitOperations.PopCount((position.WhitePawn >> 8) & whiteTerritory) * 8;
        blackScore += BitOperations.PopCount((position.BlackPawn << 8) & blackTerritory) * 8;

        // Hanging pieces are less valuable
        if (board.ToMove == PieceColour.Black)
        {
            whiteScore -= BitOperations.PopCount(blackTerritory & position.WhitePawn) * pawn / 3;
            whiteScore -= BitOperations.PopCount(blackTerritory & position.WhiteKnight) * knight / 2;
            whiteScore -= BitOperations.PopCount(blackTerritory & position.WhiteBishop) * bishop / 2;
            whiteScore -= BitOperations.PopCount(blackTerritory & position.WhiteRook) * rook / 3;
            whiteScore -= BitOperations.PopCount(blackTerritory & position.WhiteQueen) * queen / 3;
        }
        else if (board.ToMove == PieceColour.White)
        {
            blackScore -= BitOperations.PopCount(whiteTerritory & position.BlackPawn) * pawn / 3;
            blackScore -= BitOperations.PopCount(whiteTerritory & position.BlackKnight) * knight / 2;
            blackScore -= BitOperations.PopCount(whiteTerritory & position.BlackBishop) * bishop / 2;
            blackScore -= BitOperations.PopCount(whiteTerritory & position.BlackRook) * rook / 3;
            blackScore -= BitOperations.PopCount(whiteTerritory & position.BlackQueen) * queen / 3;
        }

        CalculatePawnScores(position, isEndgame, ref whiteScore, ref blackScore);

        score = whiteScore - blackScore;
        return new Score(score * (board.ToMove == PieceColour.White ? 1 : -1));
    }

    private static void CalculatePawnScores(Board board, bool isEndgame, ref int whiteScore, ref int blackScore)
    {
        ulong whitePawns = board.WhitePawn;
        ulong blackPawns = board.BlackPawn;
        int whiteFiles = 0;
        int blackFiles = 0;

        for (int i = 0; i < Chessboard.Files.Length; i++)
        {
            ulong fileMask = Chessboard.Files[i];

            if ((whitePawns & fileMask) != 0)
                whiteFiles |= 1 << i;

            if ((blackPawns & fileMask) != 0)
                blackFiles |= 1 << i;
        }

        for (int i = 0; i < Chessboard.Files.Length; i++)
        {
            int fileBit = 1 << i;
            int adjacentMask = adjacentFilesMasks[i];
            int neighbouringMask = adjacentMask ^ fileBit;
            ulong fileMask = Chessboard.Files[i];

            bool whitePawnOnFile = (whiteFiles & fileBit) != 0;
            bool blackPawnOnFile = (blackFiles & fileBit) != 0;

            if (whitePawnOnFile)
            {
                // Isolated pawns
                if ((whiteFiles & neighbouringMask) == 0) whiteScore -= 11;

                // Doubled pawns
                if (BitOperations.PopCount(whitePawns & fileMask) > 1) whiteScore -= 30;

                // Passed pawns
                if ((blackFiles & adjacentMask) == 0)
                {
                    whiteScore += 28;
                    if (isEndgame) whiteScore += 11;
                }
            }

            if (blackPawnOnFile)
            {
                // Isolated pawns
                if ((blackFiles & neighbouringMask) == 0) blackScore -= 11;

                // Doubled pawns
                if (BitOperations.PopCount(blackPawns & fileMask) > 1) blackScore -= 30;

                // Passed pawns
                if ((whiteFiles & adjacentMask) == 0)
                {
                    blackScore += 28;
                    if (isEndgame) blackScore += 11;
                }
            }
        }
    }

    private static int GetPsqtScore(ulong pieces, int[] psqt, int multiplier = 1)
    {
        int score = 0;

        while (pieces != 0)
        {
            int square = BitOperations.TrailingZeroCount(pieces);
            score += psqt[square] * multiplier;
            pieces &= pieces - 1;
        }

        return score;
    }

    private static int GetMirroredPsqtScore(ulong pieces, int[] psqt, int multiplier = 1)
    {
        int score = 0;

        while (pieces != 0)
        {
            int square = BitOperations.TrailingZeroCount(pieces);
            score += psqt[63 - square] * multiplier;
            pieces &= pieces - 1;
        }

        return score;
    }

    private static int GetWhiteMaterialCount(Board board)
    {
        int whiteMaterial = 0;
        whiteMaterial += BitOperations.PopCount(board.WhitePawn) * pawn;
        whiteMaterial += BitOperations.PopCount(board.WhiteKnight) * knight;
        whiteMaterial += BitOperations.PopCount(board.WhiteBishop) * bishop;
        whiteMaterial += BitOperations.PopCount(board.WhiteRook) * rook;
        whiteMaterial += BitOperations.PopCount(board.WhiteQueen) * queen;
        return whiteMaterial;
    }

    private static int GetBlackMaterialCount(Board board)
    {
        int blackMaterial = 0;
        blackMaterial += BitOperations.PopCount(board.BlackPawn) * pawn;
        blackMaterial += BitOperations.PopCount(board.BlackKnight) * knight;
        blackMaterial += BitOperations.PopCount(board.BlackBishop) * bishop;
        blackMaterial += BitOperations.PopCount(board.BlackRook) * rook;
        blackMaterial += BitOperations.PopCount(board.BlackQueen) * queen;
        return blackMaterial;
    }

    private static ulong GetWhiteAttacks(Board board)
    {
        ulong attacks = Magics.Neighbours[BitOperations.TrailingZeroCount(board.WhiteKing)];
        attacks |= GetPieceAttacks(board.WhiteKnight, Magics.KnightAttacks);
        attacks |= ((board.WhitePawn & ~Chessboard.AFile) >> 7) | ((board.WhitePawn & ~Chessboard.HFile) >> 9);
        attacks |= GetSlidingAttacks(board.WhiteRook | board.WhiteQueen, board.Occupied, Magics.TargetedRookAttacks);
        attacks |= GetSlidingAttacks(board.WhiteBishop | board.WhiteQueen, board.Occupied, Magics.TargetedBishopAttacks);
        return attacks;
    }

    private static ulong GetBlackAttacks(Board board)
    {
        ulong attacks = Magics.Neighbours[BitOperations.TrailingZeroCount(board.BlackKing)];
        attacks |= GetPieceAttacks(board.BlackKnight, Magics.KnightAttacks);
        attacks |= ((board.BlackPawn & ~Chessboard.AFile) << 9) | ((board.BlackPawn & ~Chessboard.HFile) << 7);
        attacks |= GetSlidingAttacks(board.BlackRook | board.BlackQueen, board.Occupied, Magics.TargetedRookAttacks);
        attacks |= GetSlidingAttacks(board.BlackBishop | board.BlackQueen, board.Occupied, Magics.TargetedBishopAttacks);
        return attacks;
    }

    private static ulong GetPieceAttacks(ulong pieces, ulong[] attacksBySquare)
    {
        ulong attacks = 0;

        while (pieces != 0)
        {
            int square = BitOperations.TrailingZeroCount(pieces);
            attacks |= attacksBySquare[square];
            pieces &= pieces - 1;
        }

        return attacks;
    }

    private static ulong GetSlidingAttacks(ulong sliders, ulong occupied, ulong[,][] attacksBySquare)
    {
        ulong attacks = 0;

        while (sliders != 0)
        {
            int square = BitOperations.TrailingZeroCount(sliders);

            for (int direction = 0; direction < 4; direction++)
            {
                ulong[] rayAttacks = attacksBySquare[square, direction];

                for (int i = 0; i < rayAttacks.Length; i++)
                {
                    ulong attackedSquare = rayAttacks[i];
                    attacks |= attackedSquare;

                    if ((attackedSquare & occupied) != 0)
                    {
                        break;
                    }
                }
            }

            sliders &= sliders - 1;
        }

        return attacks;
    }
}
