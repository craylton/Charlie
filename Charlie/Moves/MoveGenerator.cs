using Charlie.BoardRepresentation;
using System.Collections.Generic;
using System.Numerics;

namespace Charlie.Moves;

public class MoveGenerator
{
    public static IEnumerable<Move> GenerateLegalMoves(BoardState board)
    {
        foreach (Move move in GeneratePseudoLegalMoves(board))
        {
            if (!move.LeavesPlayerInCheck(board))
                yield return move;
        }
    }

    public static IEnumerable<Move> GenerateLegalMoves(SearchPosition board)
    {
        foreach (Move move in GeneratePseudoLegalMoves(board))
        {
            if (!move.LeavesPlayerInCheck(board))
                yield return move;
        }
    }

    public static IEnumerable<Move> GenerateQuiescenceMoves(BoardState board)
    {
        foreach (Move move in GeneratePseudoLegalQuiescenceMoves(board))
        {
            if (!move.LeavesPlayerInCheck(board))
                yield return move;
        }
    }

    public static IEnumerable<Move> GenerateQuiescenceMoves(SearchPosition board)
    {
        foreach (Move move in GeneratePseudoLegalQuiescenceMoves(board))
        {
            if (!move.LeavesPlayerInCheck(board))
                yield return move;
        }
    }

    public static IEnumerable<Move> GenerateLegalMoves(BoardState board, IEnumerable<Move> bestMoves)
    {
        var seenMoves = new HashSet<Move>();

        foreach (Move move in bestMoves)
        {
            if (seenMoves.Add(move))
                yield return move;
        }

        foreach (Move move in GeneratePseudoLegalMoves(board))
        {
            if (move.LeavesPlayerInCheck(board) || !seenMoves.Add(move))
                continue;

            yield return move;
        }
    }

    public static IEnumerable<Move> GenerateLegalMoves(SearchPosition board, IEnumerable<Move> bestMoves)
    {
        var seenMoves = new HashSet<Move>();

        foreach (Move move in bestMoves)
        {
            if (seenMoves.Add(move))
                yield return move;
        }

        foreach (Move move in GeneratePseudoLegalMoves(board))
        {
            if (move.LeavesPlayerInCheck(board) || !seenMoves.Add(move))
                continue;

            yield return move;
        }
    }

    public static IEnumerable<Move> TrimIllegalMoves(IEnumerable<Move> moves, BoardState board)
    {
        foreach (Move move in moves)
        {
            if (!move.LeavesPlayerInCheck(board))
                yield return move;
        }
    }

    private static IEnumerable<Move> GeneratePseudoLegalMoves(IPositionState board)
    {
        Board position = board.Board;

        foreach (Move move in GeneratePseudoLegalQuiescenceMoves(board))
            yield return move;

        if (board.ToMove == PieceColour.White)
        {
            foreach (Move move in GeneratePawnQuietMoves(position.WhitePawn, board))
                yield return move;

            foreach (Move move in GenerateBishopNonCaptures(position.WhiteBishop, position.Occupied))
                yield return move;

            foreach (Move move in GenerateRookNonCaptures(position.WhiteRook, position.Occupied))
                yield return move;

            foreach (Move move in GenerateQueenNonCaptures(position.WhiteQueen, position.Occupied, board))
                yield return move;

            foreach (Move move in GenerateKnightNonCaptures(position.WhiteKnight, position.Occupied))
                yield return move;

            foreach (Move move in GenerateKingNonCaptures(position.WhiteKing, position.Occupied, board))
                yield return move;
        }
        else
        {
            foreach (Move move in GeneratePawnQuietMoves(position.BlackPawn, board))
                yield return move;

            foreach (Move move in GenerateBishopNonCaptures(position.BlackBishop, position.Occupied))
                yield return move;

            foreach (Move move in GenerateRookNonCaptures(position.BlackRook, position.Occupied))
                yield return move;

            foreach (Move move in GenerateQueenNonCaptures(position.BlackQueen, position.Occupied, board))
                yield return move;

            foreach (Move move in GenerateKnightNonCaptures(position.BlackKnight, position.Occupied))
                yield return move;

            foreach (Move move in GenerateKingNonCaptures(position.BlackKing, position.Occupied, board))
                yield return move;
        }
    }

    private static IEnumerable<Move> GeneratePseudoLegalQuiescenceMoves(IPositionState board)
    {
        Board position = board.Board;

        if (board.ToMove == PieceColour.White)
        {
            ulong blackCapturablePieces = position.BlackPieces & ~position.BlackKing;

            foreach (Move move in GeneratePawnQuiescenceMoves(position.WhitePawn, board, blackCapturablePieces))
                yield return move;

            foreach (Move move in GenerateKnightCaptures(position.WhiteKnight, blackCapturablePieces))
                yield return move;

            foreach (Move move in GenerateBishopCaptures(position.WhiteBishop, blackCapturablePieces, board))
                yield return move;

            foreach (Move move in GenerateQueenCaptures(position.WhiteQueen, blackCapturablePieces, board))
                yield return move;

            foreach (Move move in GenerateKingCaptures(position.WhiteKing, blackCapturablePieces))
                yield return move;

            foreach (Move move in GenerateRookCaptures(position.WhiteRook, blackCapturablePieces, board))
                yield return move;
        }
        else
        {
            ulong whiteCapturablePieces = position.WhitePieces & ~position.WhiteKing;

            foreach (Move move in GeneratePawnQuiescenceMoves(position.BlackPawn, board, whiteCapturablePieces))
                yield return move;

            foreach (Move move in GenerateKnightCaptures(position.BlackKnight, whiteCapturablePieces))
                yield return move;

            foreach (Move move in GenerateBishopCaptures(position.BlackBishop, whiteCapturablePieces, board))
                yield return move;

            foreach (Move move in GenerateQueenCaptures(position.BlackQueen, whiteCapturablePieces, board))
                yield return move;

            foreach (Move move in GenerateKingCaptures(position.BlackKing, whiteCapturablePieces))
                yield return move;

            foreach (Move move in GenerateRookCaptures(position.BlackRook, whiteCapturablePieces, board))
                yield return move;
        }
    }

    private static IEnumerable<Move> GenerateKnightCaptures(ulong knights, ulong enemyPieces)
    {
        while (knights != 0)
        {
            int i = BitOperations.TrailingZeroCount(knights);
            ulong knight = 1ul << i;
            knights &= knights - 1;

            var magic = Magics.KnightAttacks[i];
            while (magic != 0)
            {
                var toSquare = 1ul << BitOperations.TrailingZeroCount(magic);

                if ((toSquare & enemyPieces) != 0)
                    yield return new Move(knight, toSquare);

                magic ^= toSquare;
            }
        }
    }

    private static IEnumerable<Move> GenerateKnightNonCaptures(ulong knights, ulong occupied)
    {
        while (knights != 0)
        {
            int i = BitOperations.TrailingZeroCount(knights);
            ulong knight = 1ul << i;
            knights &= knights - 1;

            var magic = Magics.KnightAttacks[i];
            while (magic != 0)
            {
                var toSquare = 1ul << BitOperations.TrailingZeroCount(magic);

                if ((toSquare & occupied) == 0)
                    yield return new Move(knight, toSquare);

                magic ^= toSquare;
            }
        }
    }

    private static IEnumerable<Move> GenerateQueenNonCaptures(ulong queens, ulong occupied, IPositionState board)
    {
        foreach (Move move in GenerateBishopNonCaptures(queens, occupied))
            yield return move;

        foreach (Move move in GenerateRookNonCaptures(queens, occupied))
            yield return move;
    }

    private static IEnumerable<Move> GenerateRookNonCaptures(ulong rooks, ulong occupied)
    {
        while (rooks != 0)
        {
            int i = BitOperations.TrailingZeroCount(rooks);
            ulong rook = 1ul << i;
            rooks &= rooks - 1;

            for (int direction = 0; direction < 4; direction++)
            {
                foreach (var cell in Magics.TargetedRookAttacks[i, direction])
                {
                    if ((cell & occupied) == 0)
                    {
                        yield return new Move(rook, cell);
                        continue;
                    }

                    break;
                }
            }
        }
    }

    private static IEnumerable<Move> GenerateBishopNonCaptures(ulong bishops, ulong occupied)
    {
        while (bishops != 0)
        {
            int i = BitOperations.TrailingZeroCount(bishops);
            ulong bishop = 1ul << i;
            bishops &= bishops - 1;

            for (int direction = 0; direction < 4; direction++)
            {
                foreach (var cell in Magics.TargetedBishopAttacks[i, direction])
                {
                    if ((cell & occupied) == 0)
                    {
                        yield return new Move(bishop, cell);
                        continue;
                    }

                    break;
                }
            }
        }
    }

    private static IEnumerable<Move> GenerateKingNonCaptures(ulong king, ulong occupied, IPositionState board)
    {
        Board position = board.Board;
        ulong neighbours = Magics.Neighbours[BitOperations.TrailingZeroCount(king)] & ~occupied;
        while (neighbours != 0)
        {
            var toSquare = 1ul << BitOperations.TrailingZeroCount(neighbours);
            yield return new Move(king, toSquare);
            neighbours ^= toSquare;
        }

        if (board.ToMove == PieceColour.White)
        {
            // If can short castle
            if ((board.CastleRules & 0b0001) != 0 &&
                (position.Occupied & (Chessboard.SquareF1 | Chessboard.SquareG1)) == 0 &&
                (position.WhiteRook & Chessboard.SquareH1) != 0 &&
                !board.IsInCheck(PieceColour.White) &&
                !board.IsUnderAttack(king >> 1, PieceColour.Black) &&
                !board.IsUnderAttack(king >> 2, PieceColour.Black))
            {
                yield return new Move(king, Chessboard.SquareG1, false, true, false, PromotionType.None);
            }

            // If can long castle
            if ((board.CastleRules & 0b0010) != 0 &&
                (position.Occupied & (Chessboard.SquareB1 | Chessboard.SquareC1 | Chessboard.SquareD1)) == 0 &&
                (position.WhiteRook & Chessboard.SquareA1) != 0 &&
                !board.IsInCheck(PieceColour.White) &&
                !board.IsUnderAttack(king << 1, PieceColour.Black) &&
                !board.IsUnderAttack(king << 2, PieceColour.Black))
            {
                yield return new Move(king, Chessboard.SquareC1, false, true, false, PromotionType.None);
            }
        }
        else
        {
            // If can short castle
            if ((board.CastleRules & 0b0100) != 0 &&
                (position.Occupied & (Chessboard.SquareF8 | Chessboard.SquareG8)) == 0 &&
                (position.BlackRook & Chessboard.SquareH8) != 0 &&
                !board.IsInCheck(PieceColour.Black) &&
                !board.IsUnderAttack(king >> 1, PieceColour.White) &&
                !board.IsUnderAttack(king >> 2, PieceColour.White))
            {
                yield return new Move(king, Chessboard.SquareG8, false, true, false, PromotionType.None);
            }

            // If can long castle
            if ((board.CastleRules & 0b1000) != 0 &&
                (position.Occupied & (Chessboard.SquareB8 | Chessboard.SquareC8 | Chessboard.SquareD8)) == 0 &&
                (position.BlackRook & Chessboard.SquareA8) != 0 &&
                !board.IsInCheck(PieceColour.Black) &&
                !board.IsUnderAttack(king << 1, PieceColour.White) &&
                !board.IsUnderAttack(king << 2, PieceColour.White))
            {
                yield return new Move(king, Chessboard.SquareC8, false, true, false, PromotionType.None);
            }
        }
    }

    private static IEnumerable<Move> GeneratePawnQuietMoves(ulong pawns, IPositionState board)
    {
        ulong occupiedBb = board.Board.Occupied;

        while (pawns != 0)
        {
            ulong pawn = 1ul << BitOperations.TrailingZeroCount(pawns);
            pawns &= pawns - 1;

            if (board.ToMove == PieceColour.White)
            {
                // if the pawn can move forward without promoting
                if ((pawn & Chessboard.Rank7) == 0 && ((pawn >> 8) & ~occupiedBb) != 0)
                {
                    yield return new Move(pawn, pawn >> 8);

                    // if the pawn can move a second space
                    if (((pawn >> 16) & Chessboard.Rank4 & ~occupiedBb) != 0)
                    {
                        yield return new Move(pawn, pawn >> 16, false, false, true, PromotionType.None);
                    }
                }
            }
            else
            {
                // if the pawn can move forward without promoting
                if ((pawn & Chessboard.Rank2) == 0 && ((pawn << 8) & ~occupiedBb) != 0)
                {
                    yield return new Move(pawn, pawn << 8);

                    // if the pawn can move a second space
                    if (((pawn << 16) & Chessboard.Rank5 & ~occupiedBb) != 0)
                    {
                        yield return new Move(pawn, pawn << 16, false, false, true, PromotionType.None);
                    }
                }
            }
        }
    }

    private static IEnumerable<Move> GenerateQueenCaptures(ulong queens, ulong enemyPieces, IPositionState board)
    {
        foreach (Move move in GenerateBishopCaptures(queens, enemyPieces, board))
            yield return move;

        foreach (Move move in GenerateRookCaptures(queens, enemyPieces, board))
            yield return move;
    }

    private static IEnumerable<Move> GenerateRookCaptures(ulong rooks, ulong enemyPieces, IPositionState board)
    {
        ulong occupied = board.Board.Occupied;
        while (rooks != 0)
        {
            int i = BitOperations.TrailingZeroCount(rooks);
            ulong rook = 1ul << i;
            rooks &= rooks - 1;

            for (int direction = 0; direction < 4; direction++)
            {
                foreach (var cell in Magics.TargetedRookAttacks[i, direction])
                {
                    if ((cell & enemyPieces) != 0) yield return new Move(rook, cell);
                    if ((cell & occupied) != 0) break;
                }
            }
        }
    }

    private static IEnumerable<Move> GenerateBishopCaptures(ulong bishops, ulong enemyPieces, IPositionState board)
    {
        ulong occupied = board.Board.Occupied;
        while (bishops != 0)
        {
            int i = BitOperations.TrailingZeroCount(bishops);
            ulong bishop = 1ul << i;
            bishops &= bishops - 1;

            for (int direction = 0; direction < 4; direction++)
            {
                foreach (var cell in Magics.TargetedBishopAttacks[i, direction])
                {
                    if ((cell & enemyPieces) != 0) yield return new Move(bishop, cell);
                    if ((cell & occupied) != 0) break;
                }
            }
        }
    }

    private static IEnumerable<Move> GenerateKingCaptures(ulong king, ulong enemyPieces)
    {
        bool up = (king & ~Chessboard.Rank8) != 0,
        down = (king & ~Chessboard.Rank1) != 0,
        right = (king & ~Chessboard.HFile) != 0,
        left = (king & ~Chessboard.AFile) != 0;

        // if can move up
        if (up && ((king >> 8) & enemyPieces) != 0)
            yield return new Move(king, king >> 8);

        // if can move down
        if (down && ((king << 8) & enemyPieces) != 0)
            yield return new Move(king, king << 8);

        // if can move right
        if (right && ((king >> 1) & enemyPieces) != 0)
            yield return new Move(king, king >> 1);

        // if can move left
        if (left && ((king << 1) & enemyPieces) != 0)
            yield return new Move(king, king << 1);

        // up right
        if (up && right && ((king >> 9) & enemyPieces) != 0)
            yield return new Move(king, king >> 9);

        // up left
        if (up && left && ((king >> 7) & enemyPieces) != 0)
            yield return new Move(king, king >> 7);

        // down right
        if (down && right && ((king << 7) & enemyPieces) != 0)
            yield return new Move(king, king << 7);

        // down left
        if (down && left && ((king << 9) & enemyPieces) != 0)
            yield return new Move(king, king << 9);
    }

    private static IEnumerable<Move> GeneratePawnQuiescenceMoves(ulong pawns, IPositionState board, ulong enemyPieces)
    {
        Board position = board.Board;
        ulong occupiedBb = position.Occupied;

        while (pawns != 0)
        {
            int i = BitOperations.TrailingZeroCount(pawns);
            ulong pawn = 1ul << i;
            pawns &= pawns - 1;

            if (board.ToMove == PieceColour.White)
            {
                // if the pawn can take to the left
                if (((pawn >> 7) & enemyPieces & ~Chessboard.HFile) != 0)
                {
                    // if moving forward will make it promote
                    if ((pawn & Chessboard.Rank7) != 0)
                    {
                        yield return new Move(pawn, pawn >> 7, false, false, false, PromotionType.Queen);
                        yield return new Move(pawn, pawn >> 7, false, false, false, PromotionType.Rook);
                        yield return new Move(pawn, pawn >> 7, false, false, false, PromotionType.Bishop);
                        yield return new Move(pawn, pawn >> 7, false, false, false, PromotionType.Knight);
                    }
                    else
                    {
                        yield return new Move(pawn, pawn >> 7);
                    }
                }

                // if the pawn can take to the right
                if (((pawn >> 9) & enemyPieces & ~Chessboard.AFile) != 0)
                {
                    // if moving forward will make it promote
                    if ((pawn & Chessboard.Rank7) != 0)
                    {
                        yield return new Move(pawn, pawn >> 9, false, false, false, PromotionType.Queen);
                        yield return new Move(pawn, pawn >> 9, false, false, false, PromotionType.Rook);
                        yield return new Move(pawn, pawn >> 9, false, false, false, PromotionType.Bishop);
                        yield return new Move(pawn, pawn >> 9, false, false, false, PromotionType.Knight);
                    }
                    else
                    {
                        yield return new Move(pawn, pawn >> 9);
                    }
                }

                // if can take en passant to the left
                if (((pawn >> 7) & board.WhiteEnPassant & ~Chessboard.HFile) != 0)
                {
                    yield return new Move(pawn, pawn >> 7, true, false, false, PromotionType.None);
                }

                // if can take en passant to the right
                if (((pawn >> 9) & board.WhiteEnPassant & ~Chessboard.AFile) != 0)
                {
                    yield return new Move(pawn, pawn >> 9, true, false, false, PromotionType.None);
                }

                // if the pawn can move forward to promote
                if ((pawn & Chessboard.Rank7) != 0 && ((pawn >> 8) & ~occupiedBb) != 0)
                {
                    yield return new Move(pawn, pawn >> 8, false, false, false, PromotionType.Queen);
                    yield return new Move(pawn, pawn >> 8, false, false, false, PromotionType.Rook);
                    yield return new Move(pawn, pawn >> 8, false, false, false, PromotionType.Bishop);
                    yield return new Move(pawn, pawn >> 8, false, false, false, PromotionType.Knight);
                }
            }
            else
            {
                // if the pawn can take to the left
                if (((pawn << 9) & enemyPieces & ~Chessboard.HFile) != 0)
                {
                    // if moving forward will make it promote
                    if ((pawn & Chessboard.Rank2) != 0)
                    {
                        yield return new Move(pawn, pawn << 9, false, false, false, PromotionType.Queen);
                        yield return new Move(pawn, pawn << 9, false, false, false, PromotionType.Rook);
                        yield return new Move(pawn, pawn << 9, false, false, false, PromotionType.Bishop);
                        yield return new Move(pawn, pawn << 9, false, false, false, PromotionType.Knight);
                    }
                    else
                    {
                        yield return new Move(pawn, pawn << 9);
                    }
                }

                // if the pawn can take to the right
                if (((pawn << 7) & enemyPieces & ~Chessboard.AFile) != 0)
                {
                    // if moving forward will make it promote
                    if ((pawn & Chessboard.Rank2) != 0)
                    {
                        yield return new Move(pawn, pawn << 7, false, false, false, PromotionType.Queen);
                        yield return new Move(pawn, pawn << 7, false, false, false, PromotionType.Rook);
                        yield return new Move(pawn, pawn << 7, false, false, false, PromotionType.Bishop);
                        yield return new Move(pawn, pawn << 7, false, false, false, PromotionType.Knight);
                    }
                    else
                    {
                        yield return new Move(pawn, pawn << 7);
                    }
                }

                // if can take en passant to the left
                if (((pawn << 9) & board.BlackEnPassant & ~Chessboard.HFile) != 0)
                {
                    yield return new Move(pawn, pawn << 9, true, false, false, PromotionType.None);
                }

                // if can take en passant to the right
                if (((pawn << 7) & board.BlackEnPassant & ~Chessboard.AFile) != 0)
                {
                    yield return new Move(pawn, pawn << 7, true, false, false, PromotionType.None);
                }

                // if the pawn can move forward to promote
                if ((pawn & Chessboard.Rank2) != 0 && ((pawn << 8) & ~occupiedBb) != 0)
                {
                    yield return new Move(pawn, pawn << 8, false, false, false, PromotionType.Queen);
                    yield return new Move(pawn, pawn << 8, false, false, false, PromotionType.Rook);
                    yield return new Move(pawn, pawn << 8, false, false, false, PromotionType.Bishop);
                    yield return new Move(pawn, pawn << 8, false, false, false, PromotionType.Knight);
                }
            }
        }
    }
}
