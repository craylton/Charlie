using Charlie.BoardRepresentation;
using System;
using System.Numerics;

namespace Charlie.Hash;

public static class Zobrist
{
    public static long[,] PieceSquareKeys { get; } = new long[12, 64];
    public static long SideToMoveKey { get; private set; }
    public static long[] CastlingKeys { get; } = new long[16];
    public static long[] EnPassantFileKeys { get; } = new long[8];

    public static void Initialise()
    {
        var rng = new Random(56810);

        for (int pieceType = (int)PieceType.WhiteKing; pieceType <= (int)PieceType.BlackPawn; pieceType++)
        {
            for (int cellNumber = 0; cellNumber < 64; cellNumber++)
            {
                PieceSquareKeys[pieceType, cellNumber] = RandomLong(rng);
            }
        }

        SideToMoveKey = RandomLong(rng);

        for (int castlingState = 0; castlingState < CastlingKeys.Length; castlingState++)
            CastlingKeys[castlingState] = RandomLong(rng);

        for (int file = 0; file < EnPassantFileKeys.Length; file++)
            EnPassantFileKeys[file] = RandomLong(rng);
    }

    public static long ComputePieceHash(Board board)
    {
        var hash = 0L;

        AddPieceHash(ref hash, board.WhiteKing, PieceType.WhiteKing);
        AddPieceHash(ref hash, board.BlackKing, PieceType.BlackKing);
        AddPieceHash(ref hash, board.WhiteQueen, PieceType.WhiteQueen);
        AddPieceHash(ref hash, board.BlackQueen, PieceType.BlackQueen);
        AddPieceHash(ref hash, board.WhiteRook, PieceType.WhiteRook);
        AddPieceHash(ref hash, board.BlackRook, PieceType.BlackRook);
        AddPieceHash(ref hash, board.WhiteBishop, PieceType.WhiteBishop);
        AddPieceHash(ref hash, board.BlackBishop, PieceType.BlackBishop);
        AddPieceHash(ref hash, board.WhiteKnight, PieceType.WhiteKnight);
        AddPieceHash(ref hash, board.BlackKnight, PieceType.BlackKnight);
        AddPieceHash(ref hash, board.WhitePawn, PieceType.WhitePawn);
        AddPieceHash(ref hash, board.BlackPawn, PieceType.BlackPawn);

        return hash;
    }

    public static long ComputeFullHash(
        Board board,
        PieceColour toMove,
        byte castleRules,
        ulong whiteEnPassant,
        ulong blackEnPassant)
    {
        long hash = ComputePieceHash(board);

        hash ^= CastlingKeys[castleRules];

        int enPassantFile = GetEnPassantFile(board, toMove, whiteEnPassant, blackEnPassant);
        if (enPassantFile >= 0)
            hash ^= EnPassantFileKeys[enPassantFile];

        if (toMove == PieceColour.Black)
            hash ^= SideToMoveKey;

        return hash;
    }

    public static long TogglePiece(long hash, PieceType piece, int squareIndex) =>
        hash ^ PieceSquareKeys[(int)piece, squareIndex];

    public static int GetEnPassantFile(
        Board board,
        PieceColour toMove,
        ulong whiteEnPassant,
        ulong blackEnPassant)
    {
        ulong enPassantSquare = toMove == PieceColour.White ? whiteEnPassant : blackEnPassant;

        if (enPassantSquare == 0 || !HasCapturableEnPassant(board, toMove, enPassantSquare))
            return -1;

        for (int file = 0; file < Chessboard.Files.Length; file++)
        {
            if ((enPassantSquare & Chessboard.Files[file]) != 0)
                return file;
        }

        return -1;
    }

    private static long RandomLong(Random rng)
    {
        byte[] buf = new byte[8];
        rng.NextBytes(buf);
        return BitConverter.ToInt64(buf, 0);
    }

    private static void AddPieceHash(ref long hash, ulong pieces, PieceType pieceType)
    {
        while (pieces != 0)
        {
            int cellNumber = BitOperations.TrailingZeroCount(pieces);
            hash ^= PieceSquareKeys[(int)pieceType, cellNumber];
            pieces ^= 1ul << cellNumber;
        }
    }

    private static bool HasCapturableEnPassant(Board board, PieceColour toMove, ulong enPassantSquare)
    {
        if (toMove == PieceColour.White)
        {
            bool down = (enPassantSquare & ~Chessboard.Rank1) != 0;
            bool right = (enPassantSquare & ~Chessboard.HFile) != 0;
            bool left = (enPassantSquare & ~Chessboard.AFile) != 0;

            if (down && right && ((enPassantSquare << 7) & board.WhitePawn) != 0) return true;
            if (down && left && ((enPassantSquare << 9) & board.WhitePawn) != 0) return true;
        }
        else if (toMove == PieceColour.Black)
        {
            bool up = (enPassantSquare & ~Chessboard.Rank8) != 0;
            bool right = (enPassantSquare & ~Chessboard.HFile) != 0;
            bool left = (enPassantSquare & ~Chessboard.AFile) != 0;

            if (up && right && ((enPassantSquare >> 9) & board.BlackPawn) != 0) return true;
            if (up && left && ((enPassantSquare >> 7) & board.BlackPawn) != 0) return true;
        }

        return false;
    }
}
