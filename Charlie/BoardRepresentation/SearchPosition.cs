using Charlie.Hash;
using Charlie.Moves;
using System;
using System.Numerics;

namespace Charlie.BoardRepresentation;

public class SearchPosition : IPositionState
{
    [ThreadStatic]
    private static SearchPosition cachedBoardStateSearchPosition;

    public ulong WhiteKing { get; private set; }
    public ulong BlackKing { get; private set; }

    public ulong WhiteQueen { get; private set; }
    public ulong BlackQueen { get; private set; }

    public ulong WhiteRook { get; private set; }
    public ulong BlackRook { get; private set; }

    public ulong WhiteBishop { get; private set; }
    public ulong BlackBishop { get; private set; }

    public ulong WhiteKnight { get; private set; }
    public ulong BlackKnight { get; private set; }

    public ulong WhitePawn { get; private set; }
    public ulong BlackPawn { get; private set; }

    public Board Board => new(
        WhiteKing,
        BlackKing,
        WhiteQueen,
        BlackQueen,
        WhiteRook,
        BlackRook,
        WhiteBishop,
        BlackBishop,
        WhiteKnight,
        BlackKnight,
        WhitePawn,
        BlackPawn);

    public byte CastleRules { get; private set; }

    public ulong WhiteEnPassant { get; private set; }

    public ulong BlackEnPassant { get; private set; }

    public PieceColour ToMove { get; private set; }

    public long HashCode { get; private set; }

    public SearchPosition(BoardState boardState)
    {
        Load(boardState);
    }

    private SearchPosition()
    {
    }

    internal static SearchPosition GetReusable(BoardState boardState)
    {
        SearchPosition searchPosition = cachedBoardStateSearchPosition ??= new SearchPosition();
        searchPosition.Load(boardState);
        return searchPosition;
    }

    internal void Load(BoardState boardState)
    {
        Board board = boardState.Board;
        WhiteKing = board.WhiteKing;
        BlackKing = board.BlackKing;
        WhiteQueen = board.WhiteQueen;
        BlackQueen = board.BlackQueen;
        WhiteRook = board.WhiteRook;
        BlackRook = board.BlackRook;
        WhiteBishop = board.WhiteBishop;
        BlackBishop = board.BlackBishop;
        WhiteKnight = board.WhiteKnight;
        BlackKnight = board.BlackKnight;
        WhitePawn = board.WhitePawn;
        BlackPawn = board.BlackPawn;
        CastleRules = boardState.CastleRules;
        WhiteEnPassant = boardState.WhiteEnPassant;
        BlackEnPassant = boardState.BlackEnPassant;
        ToMove = boardState.ToMove;
        HashCode = boardState.HashCode;
    }

    public void MakeMoveInPlace(Move move, ref UndoState undo)
    {
        Board currentBoard = Board;
        PieceType movingPiece = PositionUtilities.GetPieceOnSquare(currentBoard, move.FromCell);

        undo = CreateUndoState(currentBoard, move);

        byte nextCastleRules = PositionUtilities.GetUpdatedCastleRules(currentBoard, CastleRules, move);
        ulong nextWhiteEnPassant = 0;
        ulong nextBlackEnPassant = 0;

        if (move.IsDoublePush)
        {
            nextBlackEnPassant = move.ToCell << 8;
            nextWhiteEnPassant = move.ToCell >> 8;
        }

        ApplyMove(move, movingPiece);

        PieceColour nextToMove = ToMove == PieceColour.White ? PieceColour.Black : PieceColour.White;

        HashCode = UpdateHash(move, currentBoard, movingPiece, nextToMove, nextCastleRules, nextWhiteEnPassant, nextBlackEnPassant);
        CastleRules = nextCastleRules;
        WhiteEnPassant = nextWhiteEnPassant;
        BlackEnPassant = nextBlackEnPassant;
        ToMove = nextToMove;
    }

    public void UnmakeMove(Move move, in UndoState undo)
    {
        ToMove = ToMove == PieceColour.White ? PieceColour.Black : PieceColour.White;

        if (move.IsCastle)
        {
            UndoCastle(move);
        }
        else if (move.IsEnPassant)
        {
            PieceType pawn = ToMove == PieceColour.White ? PieceType.WhitePawn : PieceType.BlackPawn;
            ClearSquare(move.ToCell);
            AddPiece(pawn, move.FromCell);

            if (undo.HadCapture)
                AddPiece(undo.CapturedPiece, undo.CapturedSquare);
        }
        else if (move.PromotionType != PromotionType.None)
        {
            ClearSquare(move.ToCell);
            AddPiece(ToMove == PieceColour.White ? PieceType.WhitePawn : PieceType.BlackPawn, move.FromCell);

            if (undo.HadCapture)
                AddPiece(undo.CapturedPiece, undo.CapturedSquare);
        }
        else
        {
            PieceType movedPiece = PositionUtilities.GetPieceOnSquare(Board, move.ToCell);
            ClearSquare(move.ToCell);
            AddPiece(movedPiece, move.FromCell);

            if (undo.HadCapture)
                AddPiece(undo.CapturedPiece, undo.CapturedSquare);
        }

        CastleRules = undo.PreviousCastleRules;
        WhiteEnPassant = undo.PreviousWhiteEnPassant;
        BlackEnPassant = undo.PreviousBlackEnPassant;
        HashCode = undo.PreviousHash;
    }

    public bool IsInCheck(PieceColour toMove) => PositionUtilities.IsInCheck(Board, toMove);

    public bool IsInPseudoCheck(PieceColour attacker) => PositionUtilities.IsInPseudoCheck(Board, attacker);

    public bool IsUnderAttack(ulong cell, PieceColour attacker) => PositionUtilities.IsUnderAttack(Board, cell, attacker);

    private UndoState CreateUndoState(Board currentBoard, Move move)
    {
        if (move.IsEnPassant)
        {
            ulong capturedSquare = ToMove == PieceColour.White ? move.ToCell << 8 : move.ToCell >> 8;
            PieceType capturedPiece = ToMove == PieceColour.White ? PieceType.BlackPawn : PieceType.WhitePawn;

            return new UndoState
            {
                PreviousHash = HashCode,
                PreviousCastleRules = CastleRules,
                PreviousWhiteEnPassant = WhiteEnPassant,
                PreviousBlackEnPassant = BlackEnPassant,
                HadCapture = true,
                CapturedPiece = capturedPiece,
                CapturedSquare = capturedSquare,
            };
        }

        if ((currentBoard.Occupied & move.ToCell) != 0)
        {
            return new UndoState
            {
                PreviousHash = HashCode,
                PreviousCastleRules = CastleRules,
                PreviousWhiteEnPassant = WhiteEnPassant,
                PreviousBlackEnPassant = BlackEnPassant,
                HadCapture = true,
                CapturedPiece = PositionUtilities.GetPieceOnSquare(currentBoard, move.ToCell),
                CapturedSquare = move.ToCell,
            };
        }

        return new UndoState
        {
            PreviousHash = HashCode,
            PreviousCastleRules = CastleRules,
            PreviousWhiteEnPassant = WhiteEnPassant,
            PreviousBlackEnPassant = BlackEnPassant,
        };
    }

    private void ApplyMove(Move move, PieceType movingPiece)
    {
        if (move.IsCastle)
        {
            if (move.ToCell == Chessboard.SquareC1)
            {
                WhiteKing = move.ToCell;
                WhiteRook &= ~Chessboard.SquareA1;
                WhiteRook |= Chessboard.SquareD1;
            }
            else if (move.ToCell == Chessboard.SquareG1)
            {
                WhiteKing = move.ToCell;
                WhiteRook &= ~Chessboard.SquareH1;
                WhiteRook |= Chessboard.SquareF1;
            }
            else if (move.ToCell == Chessboard.SquareC8)
            {
                BlackKing = move.ToCell;
                BlackRook &= ~Chessboard.SquareA8;
                BlackRook |= Chessboard.SquareD8;
            }
            else if (move.ToCell == Chessboard.SquareG8)
            {
                BlackKing = move.ToCell;
                BlackRook &= ~Chessboard.SquareH8;
                BlackRook |= Chessboard.SquareF8;
            }

            return;
        }

        ClearSquare(move.FromCell);

        if (move.IsEnPassant)
        {
            ulong capturedSquare = ToMove == PieceColour.White ? move.ToCell << 8 : move.ToCell >> 8;
            ClearSquare(capturedSquare);
            AddPiece(movingPiece, move.ToCell);
            return;
        }

        ClearSquare(move.ToCell);

        if (move.PromotionType != PromotionType.None)
        {
            AddPiece(PositionUtilities.GetPromotedPieceType(ToMove, move.PromotionType), move.ToCell);
            return;
        }

        AddPiece(movingPiece, move.ToCell);
    }

    private void UndoCastle(Move move)
    {
        if (move.ToCell == Chessboard.SquareC1)
        {
            WhiteKing = move.FromCell;
            WhiteRook &= ~Chessboard.SquareD1;
            WhiteRook |= Chessboard.SquareA1;
        }
        else if (move.ToCell == Chessboard.SquareG1)
        {
            WhiteKing = move.FromCell;
            WhiteRook &= ~Chessboard.SquareF1;
            WhiteRook |= Chessboard.SquareH1;
        }
        else if (move.ToCell == Chessboard.SquareC8)
        {
            BlackKing = move.FromCell;
            BlackRook &= ~Chessboard.SquareD8;
            BlackRook |= Chessboard.SquareA8;
        }
        else if (move.ToCell == Chessboard.SquareG8)
        {
            BlackKing = move.FromCell;
            BlackRook &= ~Chessboard.SquareF8;
            BlackRook |= Chessboard.SquareH8;
        }
    }

    private long UpdateHash(
        Move move,
        Board currentBoard,
        PieceType movingPiece,
        PieceColour nextToMove,
        byte castleRules,
        ulong whiteEnPassant,
        ulong blackEnPassant)
    {
        long hash = HashCode;
        int oldEnPassantFile = Zobrist.GetEnPassantFile(currentBoard, ToMove, WhiteEnPassant, BlackEnPassant);

        if (oldEnPassantFile >= 0)
            hash ^= Zobrist.EnPassantFileKeys[oldEnPassantFile];

        hash ^= Zobrist.CastlingKeys[CastleRules];

        int fromSquare = BitOperations.TrailingZeroCount(move.FromCell);
        int toSquare = BitOperations.TrailingZeroCount(move.ToCell);

        hash = Zobrist.TogglePiece(hash, movingPiece, fromSquare);

        if (move.IsCastle)
        {
            hash = Zobrist.TogglePiece(hash, movingPiece, toSquare);

            (ulong rookFrom, ulong rookTo, PieceType rookPiece) = PositionUtilities.GetCastlingRookMove(move.ToCell);
            hash = Zobrist.TogglePiece(hash, rookPiece, BitOperations.TrailingZeroCount(rookFrom));
            hash = Zobrist.TogglePiece(hash, rookPiece, BitOperations.TrailingZeroCount(rookTo));
        }
        else
        {
            if (move.IsEnPassant)
            {
                ulong captureSquare = ToMove == PieceColour.White ? move.ToCell << 8 : move.ToCell >> 8;
                PieceType capturedPawn = ToMove == PieceColour.White ? PieceType.BlackPawn : PieceType.WhitePawn;

                hash = Zobrist.TogglePiece(hash, capturedPawn, BitOperations.TrailingZeroCount(captureSquare));
            }
            else if ((currentBoard.Occupied & move.ToCell) != 0)
            {
                PieceType capturedPiece = PositionUtilities.GetPieceOnSquare(currentBoard, move.ToCell);
                hash = Zobrist.TogglePiece(hash, capturedPiece, toSquare);
            }

            if (move.PromotionType != PromotionType.None)
            {
                PieceType promotedPiece = PositionUtilities.GetPromotedPieceType(ToMove, move.PromotionType);
                hash = Zobrist.TogglePiece(hash, promotedPiece, toSquare);
            }
            else
            {
                hash = Zobrist.TogglePiece(hash, movingPiece, toSquare);
            }
        }

        hash ^= Zobrist.CastlingKeys[castleRules];

        Board childBoard = Board;
        int newEnPassantFile = Zobrist.GetEnPassantFile(childBoard, nextToMove, whiteEnPassant, blackEnPassant);
        if (newEnPassantFile >= 0)
            hash ^= Zobrist.EnPassantFileKeys[newEnPassantFile];

        return hash ^ Zobrist.SideToMoveKey;
    }

    private void ClearSquare(ulong square)
    {
        ulong mask = ~square;
        WhiteKing &= mask;
        BlackKing &= mask;
        WhiteQueen &= mask;
        BlackQueen &= mask;
        WhiteRook &= mask;
        BlackRook &= mask;
        WhiteBishop &= mask;
        BlackBishop &= mask;
        WhiteKnight &= mask;
        BlackKnight &= mask;
        WhitePawn &= mask;
        BlackPawn &= mask;
    }

    private void AddPiece(PieceType pieceType, ulong square)
    {
        switch (pieceType)
        {
            case PieceType.WhiteKing:
                WhiteKing |= square;
                break;
            case PieceType.BlackKing:
                BlackKing |= square;
                break;
            case PieceType.WhiteQueen:
                WhiteQueen |= square;
                break;
            case PieceType.BlackQueen:
                BlackQueen |= square;
                break;
            case PieceType.WhiteRook:
                WhiteRook |= square;
                break;
            case PieceType.BlackRook:
                BlackRook |= square;
                break;
            case PieceType.WhiteBishop:
                WhiteBishop |= square;
                break;
            case PieceType.BlackBishop:
                BlackBishop |= square;
                break;
            case PieceType.WhiteKnight:
                WhiteKnight |= square;
                break;
            case PieceType.BlackKnight:
                BlackKnight |= square;
                break;
            case PieceType.WhitePawn:
                WhitePawn |= square;
                break;
            case PieceType.BlackPawn:
                BlackPawn |= square;
                break;
        }
    }
}
