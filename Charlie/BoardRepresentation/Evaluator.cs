using System.Numerics;

namespace Charlie.BoardRepresentation;

public class Evaluator
{
    private static int testValue;

    private const int pawn = 100, knight = 326, bishop = 358, rook = 519, queen = 1035;

    private static readonly int[] pawnPsqt =
    [
        0,  0,  0,  0,  0,  0,  0,  0,
        79, 70, 70, 64, 64, 70, 70, 79,
        29, 22, 32, 41, 41, 32, 22, 29,
        16, 10, 25, 17, 17, 25, 10, 16,
        4,  0,  0,  12, 12,  0, 0,  4,
        8, -5, -10, 0,  0, -10,-5,  8,
        12, 10, 10, -20,-20, 10,10, 12,
        0,  0,  0,  0,  0,  0,  0,  0,
    ];

    private static readonly int[] knightPsqt =
    [
        -40,-35,-25,-25,-25,-25,-35,-40,
        -35,-15,  1,  3,  3,  1,-15,-35,
        -30,  0, 11, 15, 15, 11,  0,-30,
        -30,  5, 16, 21, 21, 16,  5,-30,
        -30,  0, 15, 20, 20, 15,  0,-30,
        -30,  5,  9, 15, 15,  9,  5,-30,
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
        if (score > 340 || score < -340)
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

        whiteScore += BitOperations.PopCount((position.WhitePawn >> 8) & whiteTerritory) * 9;
        blackScore += BitOperations.PopCount((position.BlackPawn << 8) & blackTerritory) * 9;

        // Hanging pieces are less valuable
        if (board.ToMove == PieceColour.Black)
        {
            blackScore += 11;
            whiteScore -= BitOperations.PopCount(blackTerritory & position.WhitePawn) * pawn / 2;
            whiteScore -= BitOperations.PopCount(blackTerritory & position.WhiteKnight) * knight / 2;
            whiteScore -= BitOperations.PopCount(blackTerritory & position.WhiteBishop) * bishop / 2;
            whiteScore -= BitOperations.PopCount(blackTerritory & position.WhiteRook) * rook / 2;
            whiteScore -= BitOperations.PopCount(blackTerritory & position.WhiteQueen) * queen / 2;

            if (board.IsInPseudoCheck(PieceColour.White)) blackScore -= 6;
        }
        else if (board.ToMove == PieceColour.White)
        {
            whiteScore += 11;
            blackScore -= BitOperations.PopCount(whiteTerritory & position.BlackPawn) * pawn / 2;
            blackScore -= BitOperations.PopCount(whiteTerritory & position.BlackKnight) * knight / 2;
            blackScore -= BitOperations.PopCount(whiteTerritory & position.BlackBishop) * bishop / 2;
            blackScore -= BitOperations.PopCount(whiteTerritory & position.BlackRook) * rook / 2;
            blackScore -= BitOperations.PopCount(whiteTerritory & position.BlackQueen) * queen / 2;

            if (board.IsInPseudoCheck(PieceColour.Black)) whiteScore -= 6;
        }

        CalculatePawnScores(position, isEndgame, ref whiteScore, ref blackScore);

        score = whiteScore - blackScore;
        return new Score(score * (board.ToMove == PieceColour.White ? 1 : -1));
    }

    private static void CalculatePawnScores(Board board, bool isEndgame, ref int whiteScore, ref int blackScore)
    {
        ulong whitePawns = board.WhitePawn;
        ulong blackPawns = board.BlackPawn;
        byte whiteFiles = GetOccupiedFiles(whitePawns);
        byte blackFiles = GetOccupiedFiles(blackPawns);
        byte whiteNeighbouringFiles = GetNeighbouringFiles(whiteFiles);
        byte blackNeighbouringFiles = GetNeighbouringFiles(blackFiles);

        int whitePassedFiles = BitOperations.PopCount((uint)(whiteFiles & ~(blackFiles | blackNeighbouringFiles)));
        int blackPassedFiles = BitOperations.PopCount((uint)(blackFiles & ~(whiteFiles | whiteNeighbouringFiles)));

        // Isolated pawns
        whiteScore -= BitOperations.PopCount((uint)(whiteFiles & ~whiteNeighbouringFiles)) * 12;
        blackScore -= BitOperations.PopCount((uint)(blackFiles & ~blackNeighbouringFiles)) * 12;

        // Passed pawns
        whiteScore += whitePassedFiles * 22;
        blackScore += blackPassedFiles * 22;
        if (isEndgame)
        {
            whiteScore += whitePassedFiles * 9;
            blackScore += blackPassedFiles * 9;
        }

        int whiteTruePassedPawns = GetTruePassedPawnCount(whitePawns, blackPawns, isWhite: true);
        int blackTruePassedPawns = GetTruePassedPawnCount(blackPawns, whitePawns, isWhite: false);

        whiteScore += whiteTruePassedPawns * 15;
        blackScore += blackTruePassedPawns * 15;
        if (isEndgame)
        {
            whiteScore += whiteTruePassedPawns * 8;
            blackScore += blackTruePassedPawns * 8;
        }

        // Chained pawns
        ulong whitePawnAttacks = ((whitePawns & ~Chessboard.AFile) >> 7) | ((whitePawns & ~Chessboard.HFile) >> 9);
        ulong blackPawnAttacks = ((blackPawns & ~Chessboard.AFile) << 9) | ((blackPawns & ~Chessboard.HFile) << 7);
        whiteScore += BitOperations.PopCount(whitePawns & whitePawnAttacks) * 16;
        blackScore += BitOperations.PopCount(blackPawns & blackPawnAttacks) * 16;

    }

    private static byte GetOccupiedFiles(ulong pawns)
    {
        byte occupiedFiles = 0;

        if ((pawns & Chessboard.AFile) != 0) occupiedFiles |= 0b0000_0001;
        if ((pawns & Chessboard.BFile) != 0) occupiedFiles |= 0b0000_0010;
        if ((pawns & Chessboard.CFile) != 0) occupiedFiles |= 0b0000_0100;
        if ((pawns & Chessboard.DFile) != 0) occupiedFiles |= 0b0000_1000;
        if ((pawns & Chessboard.EFile) != 0) occupiedFiles |= 0b0001_0000;
        if ((pawns & Chessboard.FFile) != 0) occupiedFiles |= 0b0010_0000;
        if ((pawns & Chessboard.GFile) != 0) occupiedFiles |= 0b0100_0000;
        if ((pawns & Chessboard.HFile) != 0) occupiedFiles |= 0b1000_0000;

        return occupiedFiles;
    }

    private static int GetTruePassedPawnCount(ulong pawns, ulong opposingPawns, bool isWhite)
    {
        int truePassedPawns = 0;

        while (pawns != 0)
        {
            int square = BitOperations.TrailingZeroCount(pawns);
            int rank = 7 - square / 8;
            ulong pawn = 1ul << square;
            byte file = GetOccupiedFiles(pawn);
            byte relevantFiles = (byte)(file | GetNeighbouringFiles(file));
            ulong ranksAhead = isWhite ? Magics.ForwardRanks[rank] : Magics.BackwardRanks[rank];

            if ((opposingPawns & ranksAhead & GetFileMask(relevantFiles)) == 0)
                truePassedPawns++;

            pawns &= pawns - 1;
        }

        return truePassedPawns;
    }

    private static ulong GetFileMask(byte files)
    {
        ulong fileMask = 0;

        for (int i = 0; i < Chessboard.Files.Length; i++)
        {
            if ((files & (1 << i)) != 0)
                fileMask |= Chessboard.Files[i];
        }

        return fileMask;
    }

    private static byte GetNeighbouringFiles(byte files) => (byte)((files << 1) | (files >> 1));

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

    internal static void SetTestValue(int testValue) => Evaluator.testValue = testValue;
}
