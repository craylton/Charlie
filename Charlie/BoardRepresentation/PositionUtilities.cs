using Charlie.Moves;
using System;
using System.Numerics;

namespace Charlie.BoardRepresentation;

internal static class PositionUtilities
{
    internal static bool IsInCheck(Board board, PieceColour toMove)
    {
        if (toMove == PieceColour.White)
            return IsUnderAttack(board, board.WhiteKing, PieceColour.Black);

        return IsUnderAttack(board, board.BlackKing, PieceColour.White);
    }

    internal static bool IsInPseudoCheck(Board board, PieceColour attacker)
    {
        if (attacker == PieceColour.Black)
        {
            if (IsUnderImmediateAttack(board, board.WhiteKing, board.BlackKing, attacker)) return true;
            if (IsUnderKnightAttack(board.WhiteKing, board.BlackKnight)) return true;

            int cellIndex = BitOperations.TrailingZeroCount(board.WhiteKing);

            if ((Magics.AllBishopAttacks[cellIndex] & (board.BlackBishop | board.BlackQueen)) != 0) return true;
            if ((Magics.AllRookAttacks[cellIndex] & (board.BlackRook | board.BlackQueen)) != 0) return true;
        }
        else
        {
            if (IsUnderImmediateAttack(board, board.BlackKing, board.WhiteKing, attacker)) return true;
            if (IsUnderKnightAttack(board.BlackKing, board.WhiteKnight)) return true;

            int cellIndex = BitOperations.TrailingZeroCount(board.BlackKing);

            if ((Magics.AllBishopAttacks[cellIndex] & (board.WhiteBishop | board.WhiteQueen)) != 0) return true;
            if ((Magics.AllRookAttacks[cellIndex] & (board.WhiteRook | board.WhiteQueen)) != 0) return true;
        }

        return false;
    }

    internal static bool IsUnderAttack(Board board, ulong cell, PieceColour attacker)
    {
        if (cell == 0)
            return false;

        if (attacker == PieceColour.Black)
        {
            if (IsUnderImmediateAttack(board, cell, board.BlackKing, attacker)) return true;
            if (IsUnderRayAttack(board, cell, board.BlackQueen, board.BlackRook, board.BlackBishop)) return true;
            if (IsUnderKnightAttack(cell, board.BlackKnight)) return true;
        }
        else
        {
            if (IsUnderImmediateAttack(board, cell, board.WhiteKing, attacker)) return true;
            if (IsUnderRayAttack(board, cell, board.WhiteQueen, board.WhiteRook, board.WhiteBishop)) return true;
            if (IsUnderKnightAttack(cell, board.WhiteKnight)) return true;
        }

        return false;
    }

    internal static byte GetUpdatedCastleRules(Board board, byte castleRules, Move move)
    {
        byte updatedRules = castleRules;

        if ((board.WhiteRook & move.FromCell & Chessboard.SquareH1) != 0)
            updatedRules &= unchecked((byte)~0b_00000001);

        if ((board.WhiteRook & move.FromCell & Chessboard.SquareA1) != 0)
            updatedRules &= unchecked((byte)~0b_00000010);

        if ((board.WhiteKing & move.FromCell) != 0)
            updatedRules &= unchecked((byte)~0b_00000011);

        if ((board.BlackRook & move.FromCell & Chessboard.SquareH8) != 0)
            updatedRules &= unchecked((byte)~0b_00000100);

        if ((board.BlackRook & move.FromCell & Chessboard.SquareA8) != 0)
            updatedRules &= unchecked((byte)~0b_00001000);

        if ((board.BlackKing & move.FromCell) != 0)
            updatedRules &= unchecked((byte)~0b_00001100);

        if (!move.IsEnPassant)
        {
            if ((board.WhiteRook & move.ToCell & Chessboard.SquareH1) != 0)
                updatedRules &= unchecked((byte)~0b_00000001);

            if ((board.WhiteRook & move.ToCell & Chessboard.SquareA1) != 0)
                updatedRules &= unchecked((byte)~0b_00000010);

            if ((board.BlackRook & move.ToCell & Chessboard.SquareH8) != 0)
                updatedRules &= unchecked((byte)~0b_00000100);

            if ((board.BlackRook & move.ToCell & Chessboard.SquareA8) != 0)
                updatedRules &= unchecked((byte)~0b_00001000);
        }

        return updatedRules;
    }

    internal static (ulong RookFrom, ulong RookTo, PieceType RookPiece) GetCastlingRookMove(ulong kingTo)
    {
        if (kingTo == Chessboard.SquareC1)
            return (Chessboard.SquareA1, Chessboard.SquareD1, PieceType.WhiteRook);

        if (kingTo == Chessboard.SquareG1)
            return (Chessboard.SquareH1, Chessboard.SquareF1, PieceType.WhiteRook);

        if (kingTo == Chessboard.SquareC8)
            return (Chessboard.SquareA8, Chessboard.SquareD8, PieceType.BlackRook);

        if (kingTo == Chessboard.SquareG8)
            return (Chessboard.SquareH8, Chessboard.SquareF8, PieceType.BlackRook);

        throw new InvalidOperationException("Invalid castling move.");
    }

    internal static PieceType GetPieceOnSquare(Board board, ulong square)
    {
        if ((board.WhiteKing & square) != 0) return PieceType.WhiteKing;
        if ((board.BlackKing & square) != 0) return PieceType.BlackKing;
        if ((board.WhiteQueen & square) != 0) return PieceType.WhiteQueen;
        if ((board.BlackQueen & square) != 0) return PieceType.BlackQueen;
        if ((board.WhiteRook & square) != 0) return PieceType.WhiteRook;
        if ((board.BlackRook & square) != 0) return PieceType.BlackRook;
        if ((board.WhiteBishop & square) != 0) return PieceType.WhiteBishop;
        if ((board.BlackBishop & square) != 0) return PieceType.BlackBishop;
        if ((board.WhiteKnight & square) != 0) return PieceType.WhiteKnight;
        if ((board.BlackKnight & square) != 0) return PieceType.BlackKnight;
        if ((board.WhitePawn & square) != 0) return PieceType.WhitePawn;
        if ((board.BlackPawn & square) != 0) return PieceType.BlackPawn;

        throw new InvalidOperationException("No piece on square.");
    }

    internal static PieceType GetPromotedPieceType(PieceColour pieceColour, PromotionType promotionType)
    {
        return (pieceColour, promotionType) switch
        {
            (PieceColour.White, PromotionType.Queen) => PieceType.WhiteQueen,
            (PieceColour.Black, PromotionType.Queen) => PieceType.BlackQueen,
            (PieceColour.White, PromotionType.Rook) => PieceType.WhiteRook,
            (PieceColour.Black, PromotionType.Rook) => PieceType.BlackRook,
            (PieceColour.White, PromotionType.Bishop) => PieceType.WhiteBishop,
            (PieceColour.Black, PromotionType.Bishop) => PieceType.BlackBishop,
            (PieceColour.White, PromotionType.Knight) => PieceType.WhiteKnight,
            (PieceColour.Black, PromotionType.Knight) => PieceType.BlackKnight,
            _ => throw new InvalidOperationException("Invalid promotion type."),
        };
    }

    private static bool IsUnderImmediateAttack(Board board, ulong cell, ulong theirKing, PieceColour attacker)
    {
        if (cell == 0)
            return false;

        int cellIndex = BitOperations.TrailingZeroCount(cell);
        ulong neighbours = Magics.Neighbours[cellIndex];

        if ((neighbours & theirKing) != 0)
            return true;

        ulong pawnAttacks = attacker == PieceColour.Black
            ? Magics.WhitePawnAttacks[cellIndex]
            : Magics.BlackPawnAttacks[cellIndex];

        ulong theirPawns = attacker == PieceColour.Black
            ? board.BlackPawn
            : board.WhitePawn;

        return (pawnAttacks & theirPawns) != 0;
    }

    private static bool IsUnderRayAttack(Board board, ulong cell, ulong theirQueen, ulong theirRook, ulong theirBishop)
    {
        if (cell == 0)
            return false;

        int cellIndex = BitOperations.TrailingZeroCount(cell);
        ulong occupiedBb = board.Occupied;
        ulong ordinalSliders = theirRook | theirQueen;
        ulong diagonalSliders = theirBishop | theirQueen;

        if ((Magics.AllRookAttacks[cellIndex] & ordinalSliders) != 0)
        {
            for (int direction = 0; direction < 4; direction++)
            {
                foreach (var c in Magics.TargetedRookAttacks[cellIndex, direction])
                {
                    if ((c & ordinalSliders) != 0) return true;
                    if ((c & occupiedBb) != 0) break;
                }
            }
        }

        if ((Magics.AllBishopAttacks[cellIndex] & diagonalSliders) != 0)
        {
            for (int direction = 0; direction < 4; direction++)
            {
                foreach (var c in Magics.TargetedBishopAttacks[cellIndex, direction])
                {
                    if ((c & diagonalSliders) != 0) return true;
                    if ((c & occupiedBb) != 0) break;
                }
            }
        }

        return false;
    }

    private static bool IsUnderKnightAttack(ulong cell, ulong theirKnight)
    {
        if (cell == 0)
            return false;

        int cellIndex = BitOperations.TrailingZeroCount(cell);
        return (Magics.KnightAttacks[cellIndex] & theirKnight) != 0;
    }
}
