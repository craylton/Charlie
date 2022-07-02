using Charlie.BoardRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Charlie.Moves;

public readonly struct Move : IEquatable<Move>
{
    public ulong FromCell { get; }

    public ulong ToCell { get; }

    public bool IsEnPassant { get; }

    public bool IsCastle { get; }

    public bool IsDoublePush { get; }

    public PromotionType PromotionType { get; }

    public Move(
        ulong fromCell,
        ulong toCell,
        bool isEnPassant,
        bool isCastle,
        bool isDoublePush,
        PromotionType promotionType) =>
        (FromCell, ToCell, IsEnPassant, IsCastle, IsDoublePush, PromotionType) =
        (fromCell, toCell, isEnPassant, isCastle, isDoublePush, promotionType);

    public Move(ulong fromCell, ulong toCell) :
        this(fromCell, toCell, false, false, false, PromotionType.None)
    {
    }

    public override string ToString()
    {
        string from = Chessboard.CellNames[BitOperations.LeadingZeroCount(FromCell)];
        string to = Chessboard.CellNames[BitOperations.LeadingZeroCount(ToCell)];

        var promotion = string.Empty;

        if (PromotionType != PromotionType.None)
            promotion = "=" + PromotionType.GetSuffix();

        return $"{from}{to}{promotion}";
    }

    public static Move FromString(IEnumerable<Move> possibleMoves, string move)
    {
        var from = new string(move.Take(2).ToArray());
        var to = new string(new string(move.Take(4).ToArray()).TakeLast(2).ToArray());

        ulong fromCell = 0, toCell = 0;

        for (int i = 0; i < Chessboard.CellNames.Length; i++)
        {
            if (from.ToUpper() == Chessboard.CellNames[i].ToUpper())
                fromCell = 1ul << (63 - i);

            if (to.ToUpper() == Chessboard.CellNames[i].ToUpper())
                toCell = 1ul << (63 - i);
        }

        IEnumerable<Move> matches = possibleMoves.Where(m => m.FromCell == fromCell && m.ToCell == toCell);

        // If it was a pawn promotion, there will be multiple matching moves
        if (matches.Count() > 1 && move.Length >= 5)
        {
            string promotion = move[^1].ToString().ToUpper();
            return matches.Single(m => m.PromotionType.GetSuffix().ToUpper() == promotion);
        }

        return matches.Single();
    }

    public bool IsCapture(BoardState board) => (board.Board.Occupied & ToCell) != 0;

    public bool IsCaptureOrPromotion(BoardState board) =>
        IsCapture(board) || PromotionType != PromotionType.None;

    public bool IsAdvancedPawnPush(BoardState board)
    {
        var whiteAdvancePush = Chessboard.Rank5 | Chessboard.Rank6 | Chessboard.Rank7;
        if (board.ToMove == PieceColour.White && (board.Board.WhitePawn & FromCell & whiteAdvancePush) != 0)
            return true;

        var blackAdvancePush = Chessboard.Rank4 | Chessboard.Rank3 | Chessboard.Rank2;
        if (board.ToMove == PieceColour.Black && (board.Board.BlackPawn & FromCell & blackAdvancePush) != 0)
            return true;

        return false;
    }

    public bool IsValidMove() => !Equals(default);

    public bool LeavesPlayerInCheck(BoardState board)
    {
        PieceColour attacker = board.ToMove == PieceColour.White ? PieceColour.Black : PieceColour.White;

        BoardState newState = board.MakeMove(this);
        // Look if there are any enemy pieces aimed at the king
        if (newState.IsInPseudoCheck(attacker))
            return newState.IsInCheck(board.ToMove);

        return false;
    }

    public bool Equals(Move other) =>
        FromCell == other.FromCell &&
        ToCell == other.ToCell &&
        IsEnPassant == other.IsEnPassant &&
        IsCastle == other.IsCastle &&
        IsDoublePush == other.IsDoublePush &&
        PromotionType == other.PromotionType;

    public override bool Equals(object obj) =>
        obj is Move move && Equals(move);

    public override int GetHashCode() =>
        HashCode.Combine(FromCell, ToCell, IsEnPassant, IsCastle, IsDoublePush, PromotionType);

    public static bool operator ==(Move left, Move right) => left.Equals(right);

    public static bool operator !=(Move left, Move right) => !(left == right);
}
