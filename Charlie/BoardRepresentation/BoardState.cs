using Charlie.Hash;
using Charlie.Moves;
using System;
using System.Diagnostics;
using System.Numerics;

namespace Charlie.BoardRepresentation;

public class BoardState
{
    private static string StartPositionFen { get; } =
        "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    private readonly HistoryNode previousStates;

    private sealed record HistoryNode(long Hash, HistoryNode Previous);

    public Board Board { get; }

    // 0001 = white short, 0010 = white long, 0100 = black short, 1000 = black long
    public byte CastleRules { get; }

    // For en-passants, the set bit is where the capturing pawn will end up. White = white can capture
    public ulong WhiteEnPassant { get; }
    public ulong BlackEnPassant { get; }

    public PieceColour ToMove { get; }

    public long HashCode { get; }

    public BoardState() : this(StartPositionFen.Split(" "))
    {
    }

    private BoardState(
        HistoryNode previousStates,
        Board bitBoard,
        PieceColour toMove,
        byte castleRules,
        ulong whiteEnPassant,
        ulong blackEnPassant,
        long hashCode)
    {
        Board = bitBoard;

        CastleRules = castleRules;

        WhiteEnPassant = whiteEnPassant;
        BlackEnPassant = blackEnPassant;

        ToMove = toMove;

        HashCode = hashCode;
        this.previousStates = new HistoryNode(HashCode, previousStates);
    }

    public BoardState(string[] fenElements)
    {
        string pieces = fenElements[0];
        string toMove = fenElements[1];
        string castlingRules = fenElements[2];
        string enPassant = fenElements[3];
        string fiftyMoveRule = fenElements[4];
        string numberOfMoves = fenElements[5];

        Board = new Board(pieces);
        CastleRules = GetCastlingRulesFromFen(castlingRules);
        ToMove = toMove == "w" ? PieceColour.White : PieceColour.Black;
        WhiteEnPassant = GetEnPassantFromFen(enPassant[0], ToMove == PieceColour.White);
        BlackEnPassant = GetEnPassantFromFen(enPassant[0], ToMove == PieceColour.Black);
        HashCode = CalculateLongHashCode();
        previousStates = new HistoryNode(HashCode, null);
    }

    private static ulong GetEnPassantFromFen(char enPassantFile, bool whiteToMove)
    {
        if (enPassantFile == '-') return 0;

        int rank = whiteToMove ? 3 : 6;
        int file = enPassantFile - 'a';
        return 1ul << (8 * rank - file - 1);
    }

    private static byte GetCastlingRulesFromFen(string fenCastling)
    {
        byte castlingRules = 0;

        if (fenCastling != "-")
        {
            foreach (char c in fenCastling)
            {
                if (c == 'K') castlingRules |= 0b0000_0001;
                if (c == 'Q') castlingRules |= 0b0000_0010;
                if (c == 'k') castlingRules |= 0b0000_0100;
                if (c == 'q') castlingRules |= 0b0000_1000;
            }
        }

        return castlingRules;
    }

    public BoardState MakeMove(Move move)
    {
        ulong whiteEP = 0, blackEP = 0;

        if (move.IsDoublePush)
        {
            blackEP = move.ToCell << 8;
            whiteEP = move.ToCell >> 8;
        }

        byte castleRules = GetUpdatedCastleRules(move);

        PieceColour nextToMove = ToMove == PieceColour.White ? PieceColour.Black : PieceColour.White;
        Board newBoard = new(Board, move);
        long childHash = UpdateHash(move, newBoard, nextToMove, castleRules, whiteEP, blackEP);

#if DEBUG
        Debug.Assert(childHash == Zobrist.ComputeFullHash(newBoard, nextToMove, castleRules, whiteEP, blackEP));
#endif

        return new BoardState(
            previousStates,
            newBoard,
            nextToMove,
            castleRules,
            whiteEP,
            blackEP,
            childHash);
    }

    private byte GetUpdatedCastleRules(Move move)
    {
        byte castleRules = CastleRules;

        if ((Board.WhiteRook & move.FromCell & Chessboard.SquareH1) != 0)
            castleRules &= unchecked((byte)~0b_00000001);

        if ((Board.WhiteRook & move.FromCell & Chessboard.SquareA1) != 0)
            castleRules &= unchecked((byte)~0b_00000010);

        if ((Board.WhiteKing & move.FromCell) != 0)
            castleRules &= unchecked((byte)~0b_00000011);

        if ((Board.BlackRook & move.FromCell & Chessboard.SquareH8) != 0)
            castleRules &= unchecked((byte)~0b_00000100);

        if ((Board.BlackRook & move.FromCell & Chessboard.SquareA8) != 0)
            castleRules &= unchecked((byte)~0b_00001000);

        if ((Board.BlackKing & move.FromCell) != 0)
            castleRules &= unchecked((byte)~0b_00001100);

        if (!move.IsEnPassant)
        {
            if ((Board.WhiteRook & move.ToCell & Chessboard.SquareH1) != 0)
                castleRules &= unchecked((byte)~0b_00000001);

            if ((Board.WhiteRook & move.ToCell & Chessboard.SquareA1) != 0)
                castleRules &= unchecked((byte)~0b_00000010);

            if ((Board.BlackRook & move.ToCell & Chessboard.SquareH8) != 0)
                castleRules &= unchecked((byte)~0b_00000100);

            if ((Board.BlackRook & move.ToCell & Chessboard.SquareA8) != 0)
                castleRules &= unchecked((byte)~0b_00001000);
        }

        return castleRules;
    }

    private long UpdateHash(
        Move move,
        Board childBoard,
        PieceColour nextToMove,
        byte castleRules,
        ulong whiteEnPassant,
        ulong blackEnPassant)
    {
        long hash = HashCode;
        int oldEnPassantFile = Zobrist.GetEnPassantFile(Board, ToMove, WhiteEnPassant, BlackEnPassant);

        if (oldEnPassantFile >= 0)
            hash ^= Zobrist.EnPassantFileKeys[oldEnPassantFile];

        hash ^= Zobrist.CastlingKeys[CastleRules];

        PieceType movingPiece = GetPieceOnSquare(Board, move.FromCell);
        int fromSquare = BitOperations.TrailingZeroCount(move.FromCell);
        int toSquare = BitOperations.TrailingZeroCount(move.ToCell);

        hash = Zobrist.TogglePiece(hash, movingPiece, fromSquare);

        if (move.IsCastle)
        {
            hash = Zobrist.TogglePiece(hash, movingPiece, toSquare);

            (ulong rookFrom, ulong rookTo, PieceType rookPiece) = GetCastlingRookMove(move.ToCell);
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
            else if ((Board.Occupied & move.ToCell) != 0)
            {
                PieceType capturedPiece = GetPieceOnSquare(Board, move.ToCell);
                hash = Zobrist.TogglePiece(hash, capturedPiece, toSquare);
            }

            if (move.PromotionType != PromotionType.None)
            {
                PieceType promotedPiece = GetPromotedPieceType(ToMove, move.PromotionType);
                hash = Zobrist.TogglePiece(hash, promotedPiece, toSquare);
            }
            else
            {
                hash = Zobrist.TogglePiece(hash, movingPiece, toSquare);
            }
        }

        hash ^= Zobrist.CastlingKeys[castleRules];

        int newEnPassantFile = Zobrist.GetEnPassantFile(childBoard, nextToMove, whiteEnPassant, blackEnPassant);
        if (newEnPassantFile >= 0)
            hash ^= Zobrist.EnPassantFileKeys[newEnPassantFile];

        return hash ^ Zobrist.SideToMoveKey;
    }

    private static (ulong RookFrom, ulong RookTo, PieceType RookPiece) GetCastlingRookMove(ulong kingTo)
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

    private static PieceType GetPieceOnSquare(Board board, ulong square)
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

    private static PieceType GetPromotedPieceType(PieceColour pieceColour, PromotionType promotionType)
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

    internal bool IsThreeMoveRepetition()
    {
        int count = 0;

        for (HistoryNode state = previousStates; state is not null; state = state.Previous)
        {
            if (state.Hash.Equals(HashCode))
            {
                count++;

                if (count == 3)
                    return true;
            }
        }

        return false;
    }

    internal bool IsInCheck(PieceColour toMove)
    {
        if (toMove == PieceColour.White)
            return IsUnderAttack(Board.WhiteKing, PieceColour.Black);
        else
            return IsUnderAttack(Board.BlackKing, PieceColour.White);
    }

    internal bool IsInPseudoCheck(PieceColour attacker)
    {
        if (attacker == PieceColour.Black)
        {
            if (IsUnderImmediateAttack(Board.WhiteKing, Board.BlackKing, attacker)) return true;
            if (IsUnderKnightAttack(Board.WhiteKing, Board.BlackKnight)) return true;

            int cellIndex = BitOperations.TrailingZeroCount(Board.WhiteKing);

            if ((Magics.AllBishopAttacks[cellIndex] & (Board.BlackBishop | Board.BlackQueen)) != 0) return true;
            if ((Magics.AllRookAttacks[cellIndex] & (Board.BlackRook | Board.BlackQueen)) != 0) return true;
        }
        else
        {
            if (IsUnderImmediateAttack(Board.BlackKing, Board.WhiteKing, attacker)) return true;
            if (IsUnderKnightAttack(Board.BlackKing, Board.WhiteKnight)) return true;

            int cellIndex = BitOperations.TrailingZeroCount(Board.BlackKing);

            if ((Magics.AllBishopAttacks[cellIndex] & (Board.WhiteBishop | Board.WhiteQueen)) != 0) return true;
            if ((Magics.AllRookAttacks[cellIndex] & (Board.WhiteRook | Board.WhiteQueen)) != 0) return true;
        }

        return false;
    }

    internal bool IsUnderAttack(ulong cell, PieceColour attacker)
    {
        if (attacker == PieceColour.Black)
        {
            if (IsUnderImmediateAttack(cell, Board.BlackKing, attacker)) return true;
            if (IsUnderRayAttack(cell, Board.BlackQueen, Board.BlackRook, Board.BlackBishop)) return true;
            if (IsUnderKnightAttack(cell, Board.BlackKnight)) return true;
        }
        else
        {
            if (IsUnderImmediateAttack(cell, Board.WhiteKing, attacker)) return true;
            if (IsUnderRayAttack(cell, Board.WhiteQueen, Board.WhiteRook, Board.WhiteBishop)) return true;
            if (IsUnderKnightAttack(cell, Board.WhiteKnight)) return true;
        }

        return false;
    }

    private bool IsUnderImmediateAttack(ulong cell, ulong theirKing, PieceColour attacker)
    {
        bool up = (cell & ~Chessboard.Rank8) != 0,
        down = (cell & ~Chessboard.Rank1) != 0,
        right = (cell & ~Chessboard.HFile) != 0,
        left = (cell & ~Chessboard.AFile) != 0;

        ulong neighbours = Magics.Neighbours[BitOperations.TrailingZeroCount(cell)];

        if ((neighbours & theirKing) != 0)
            return true;

        if (attacker == PieceColour.Black && (neighbours & Board.BlackPawn) != 0)
        {
            if (up && right && ((cell >> 9) & Board.BlackPawn) != 0) return true;
            if (up && left && ((cell >> 7) & Board.BlackPawn) != 0) return true;
        }
        else if (attacker == PieceColour.White && (neighbours & Board.WhitePawn) != 0)
        {
            if (down && right && ((cell << 7) & Board.WhitePawn) != 0) return true;
            if (down && left && ((cell << 9) & Board.WhitePawn) != 0) return true;
        }

        return false;
    }

    private bool IsUnderRayAttack(ulong cell, ulong theirQueen, ulong theirRook, ulong theirBishop)
    {
        int cellIndex = BitOperations.TrailingZeroCount(cell);
        ulong occupiedBb = Board.Occupied;
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
        int cellIndex = BitOperations.TrailingZeroCount(cell);
        return (Magics.KnightAttacks[cellIndex] & theirKnight) != 0;
    }

    private long CalculateLongHashCode() =>
        Zobrist.ComputeFullHash(Board, ToMove, CastleRules, WhiteEnPassant, BlackEnPassant);
}
