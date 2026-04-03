using Charlie.BoardRepresentation;
using System.Collections.Generic;
using System.Numerics;

namespace Charlie.Moves;

public class MoveGenerator
{
    public static IEnumerable<Move> GenerateLegalMoves(BoardState board)
    {
        List<Move> legalMoves = new(64);

        foreach (Move move in GeneratePseudoLegalMoves(board))
        {
            if (!move.LeavesPlayerInCheck(board))
                legalMoves.Add(move);
        }

        return legalMoves;
    }

    public static IEnumerable<Move> GenerateLegalMoves(SearchPosition board)
    {
        List<Move> legalMoves = new(64);

        foreach (Move move in GeneratePseudoLegalMoves(board))
        {
            if (!move.LeavesPlayerInCheck(board))
                legalMoves.Add(move);
        }

        return legalMoves;
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

    public static IEnumerable<Move> GenerateLegalMoves(
        SearchPosition board,
        Move firstBestMove,
        bool hasFirstBestMove,
        Move secondBestMove,
        bool hasSecondBestMove)
    {
        if (hasFirstBestMove)
            yield return firstBestMove;

        if (hasSecondBestMove && (!hasFirstBestMove || !secondBestMove.Equals(firstBestMove)))
            yield return secondBestMove;

        foreach (Move move in GeneratePseudoLegalMoves(board))
        {
            if (move.LeavesPlayerInCheck(board))
                continue;

            if (hasFirstBestMove && move.Equals(firstBestMove))
                continue;

            if (hasSecondBestMove && move.Equals(secondBestMove))
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
        List<Move> moves = new(64);

        AddPseudoLegalQuiescenceMoves(moves, board);

        if (board.ToMove == PieceColour.White)
        {
            AddPawnQuietMoves(moves, position.WhitePawn, board);
            AddBishopNonCaptures(moves, position.WhiteBishop, position.Occupied);
            AddRookNonCaptures(moves, position.WhiteRook, position.Occupied);
            AddQueenNonCaptures(moves, position.WhiteQueen, position.Occupied);
            AddKnightNonCaptures(moves, position.WhiteKnight, position.Occupied);
            AddKingNonCaptures(moves, position.WhiteKing, position.Occupied, board);
        }
        else
        {
            AddPawnQuietMoves(moves, position.BlackPawn, board);
            AddBishopNonCaptures(moves, position.BlackBishop, position.Occupied);
            AddRookNonCaptures(moves, position.BlackRook, position.Occupied);
            AddQueenNonCaptures(moves, position.BlackQueen, position.Occupied);
            AddKnightNonCaptures(moves, position.BlackKnight, position.Occupied);
            AddKingNonCaptures(moves, position.BlackKing, position.Occupied, board);
        }

        return moves;
    }

    private static IEnumerable<Move> GeneratePseudoLegalQuiescenceMoves(IPositionState board)
    {
        List<Move> moves = new(32);
        AddPseudoLegalQuiescenceMoves(moves, board);
        return moves;
    }

    private static void AddPseudoLegalQuiescenceMoves(List<Move> moves, IPositionState board)
    {
        Board position = board.Board;

        if (board.ToMove == PieceColour.White)
        {
            ulong blackCapturablePieces = position.BlackPieces & ~position.BlackKing;

            AddPawnQuiescenceMoves(moves, position.WhitePawn, board, blackCapturablePieces);
            AddKnightCaptures(moves, position.WhiteKnight, blackCapturablePieces);
            AddBishopCaptures(moves, position.WhiteBishop, blackCapturablePieces, board);
            AddQueenCaptures(moves, position.WhiteQueen, blackCapturablePieces, board);
            AddKingCaptures(moves, position.WhiteKing, blackCapturablePieces);
            AddRookCaptures(moves, position.WhiteRook, blackCapturablePieces, board);
        }
        else
        {
            ulong whiteCapturablePieces = position.WhitePieces & ~position.WhiteKing;

            AddPawnQuiescenceMoves(moves, position.BlackPawn, board, whiteCapturablePieces);
            AddKnightCaptures(moves, position.BlackKnight, whiteCapturablePieces);
            AddBishopCaptures(moves, position.BlackBishop, whiteCapturablePieces, board);
            AddQueenCaptures(moves, position.BlackQueen, whiteCapturablePieces, board);
            AddKingCaptures(moves, position.BlackKing, whiteCapturablePieces);
            AddRookCaptures(moves, position.BlackRook, whiteCapturablePieces, board);
        }
    }

    private static void AddKnightCaptures(List<Move> moves, ulong knights, ulong enemyPieces)
    {
        while (knights != 0)
        {
            int i = BitOperations.TrailingZeroCount(knights);
            ulong knight = 1ul << i;
            knights &= knights - 1;

            ulong targets = Magics.KnightAttacks[i] & enemyPieces;
            while (targets != 0)
            {
                ulong toSquare = 1ul << BitOperations.TrailingZeroCount(targets);
                moves.Add(new Move(knight, toSquare));
                targets ^= toSquare;
            }
        }
    }

    private static void AddKnightNonCaptures(List<Move> moves, ulong knights, ulong occupied)
    {
        while (knights != 0)
        {
            int i = BitOperations.TrailingZeroCount(knights);
            ulong knight = 1ul << i;
            knights &= knights - 1;

            ulong targets = Magics.KnightAttacks[i] & ~occupied;
            while (targets != 0)
            {
                ulong toSquare = 1ul << BitOperations.TrailingZeroCount(targets);
                moves.Add(new Move(knight, toSquare));
                targets ^= toSquare;
            }
        }
    }

    private static void AddQueenNonCaptures(List<Move> moves, ulong queens, ulong occupied)
    {
        AddBishopNonCaptures(moves, queens, occupied);
        AddRookNonCaptures(moves, queens, occupied);
    }

    private static void AddRookNonCaptures(List<Move> moves, ulong rooks, ulong occupied)
    {
        while (rooks != 0)
        {
            int i = BitOperations.TrailingZeroCount(rooks);
            ulong rook = 1ul << i;
            rooks &= rooks - 1;

            for (int direction = 0; direction < 4; direction++)
            {
                ulong[] ray = Magics.TargetedRookAttacks[i, direction];
                for (int j = 0; j < ray.Length; j++)
                {
                    ulong cell = ray[j];
                    if ((cell & occupied) == 0)
                    {
                        moves.Add(new Move(rook, cell));
                        continue;
                    }

                    break;
                }
            }
        }
    }

    private static void AddBishopNonCaptures(List<Move> moves, ulong bishops, ulong occupied)
    {
        while (bishops != 0)
        {
            int i = BitOperations.TrailingZeroCount(bishops);
            ulong bishop = 1ul << i;
            bishops &= bishops - 1;

            for (int direction = 0; direction < 4; direction++)
            {
                ulong[] ray = Magics.TargetedBishopAttacks[i, direction];
                for (int j = 0; j < ray.Length; j++)
                {
                    ulong cell = ray[j];
                    if ((cell & occupied) == 0)
                    {
                        moves.Add(new Move(bishop, cell));
                        continue;
                    }

                    break;
                }
            }
        }
    }

    private static void AddKingNonCaptures(List<Move> moves, ulong king, ulong occupied, IPositionState board)
    {
        Board position = board.Board;
        ulong neighbours = Magics.Neighbours[BitOperations.TrailingZeroCount(king)] & ~occupied;
        PieceColour toMove = board.ToMove;
        PieceColour attacker = toMove == PieceColour.White ? PieceColour.Black : PieceColour.White;

        while (neighbours != 0)
        {
            ulong toSquare = 1ul << BitOperations.TrailingZeroCount(neighbours);
            moves.Add(new Move(king, toSquare));
            neighbours ^= toSquare;
        }

        bool canCastle = board.CastleRules != 0;
        bool isInCheck = canCastle && board.IsInCheck(toMove);

        if (toMove == PieceColour.White)
        {
            // If can short castle
            if ((board.CastleRules & 0b0001) != 0 &&
                (position.Occupied & (Chessboard.SquareF1 | Chessboard.SquareG1)) == 0 &&
                (position.WhiteRook & Chessboard.SquareH1) != 0 &&
                !isInCheck &&
                !board.IsUnderAttack(king >> 1, attacker) &&
                !board.IsUnderAttack(king >> 2, attacker))
            {
                moves.Add(new Move(king, Chessboard.SquareG1, false, true, false, PromotionType.None));
            }

            // If can long castle
            if ((board.CastleRules & 0b0010) != 0 &&
                (position.Occupied & (Chessboard.SquareB1 | Chessboard.SquareC1 | Chessboard.SquareD1)) == 0 &&
                (position.WhiteRook & Chessboard.SquareA1) != 0 &&
                !isInCheck &&
                !board.IsUnderAttack(king << 1, attacker) &&
                !board.IsUnderAttack(king << 2, attacker))
            {
                moves.Add(new Move(king, Chessboard.SquareC1, false, true, false, PromotionType.None));
            }
        }
        else
        {
            // If can short castle
            if ((board.CastleRules & 0b0100) != 0 &&
                (position.Occupied & (Chessboard.SquareF8 | Chessboard.SquareG8)) == 0 &&
                (position.BlackRook & Chessboard.SquareH8) != 0 &&
                !isInCheck &&
                !board.IsUnderAttack(king >> 1, attacker) &&
                !board.IsUnderAttack(king >> 2, attacker))
            {
                moves.Add(new Move(king, Chessboard.SquareG8, false, true, false, PromotionType.None));
            }

            // If can long castle
            if ((board.CastleRules & 0b1000) != 0 &&
                (position.Occupied & (Chessboard.SquareB8 | Chessboard.SquareC8 | Chessboard.SquareD8)) == 0 &&
                (position.BlackRook & Chessboard.SquareA8) != 0 &&
                !isInCheck &&
                !board.IsUnderAttack(king << 1, attacker) &&
                !board.IsUnderAttack(king << 2, attacker))
            {
                moves.Add(new Move(king, Chessboard.SquareC8, false, true, false, PromotionType.None));
            }
        }
    }

    private static void AddPawnQuietMoves(List<Move> moves, ulong pawns, IPositionState board)
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
                    moves.Add(new Move(pawn, pawn >> 8));

                    // if the pawn can move a second space
                    if (((pawn >> 16) & Chessboard.Rank4 & ~occupiedBb) != 0)
                    {
                        moves.Add(new Move(pawn, pawn >> 16, false, false, true, PromotionType.None));
                    }
                }
            }
            else
            {
                // if the pawn can move forward without promoting
                if ((pawn & Chessboard.Rank2) == 0 && ((pawn << 8) & ~occupiedBb) != 0)
                {
                    moves.Add(new Move(pawn, pawn << 8));

                    // if the pawn can move a second space
                    if (((pawn << 16) & Chessboard.Rank5 & ~occupiedBb) != 0)
                    {
                        moves.Add(new Move(pawn, pawn << 16, false, false, true, PromotionType.None));
                    }
                }
            }
        }
    }

    private static void AddQueenCaptures(List<Move> moves, ulong queens, ulong enemyPieces, IPositionState board)
    {
        AddBishopCaptures(moves, queens, enemyPieces, board);
        AddRookCaptures(moves, queens, enemyPieces, board);
    }

    private static void AddRookCaptures(List<Move> moves, ulong rooks, ulong enemyPieces, IPositionState board)
    {
        ulong occupied = board.Board.Occupied;
        while (rooks != 0)
        {
            int i = BitOperations.TrailingZeroCount(rooks);
            ulong rook = 1ul << i;
            rooks &= rooks - 1;

            for (int direction = 0; direction < 4; direction++)
            {
                ulong[] ray = Magics.TargetedRookAttacks[i, direction];
                for (int j = 0; j < ray.Length; j++)
                {
                    ulong cell = ray[j];
                    if ((cell & enemyPieces) != 0) moves.Add(new Move(rook, cell));
                    if ((cell & occupied) != 0) break;
                }
            }
        }
    }

    private static void AddBishopCaptures(List<Move> moves, ulong bishops, ulong enemyPieces, IPositionState board)
    {
        ulong occupied = board.Board.Occupied;
        while (bishops != 0)
        {
            int i = BitOperations.TrailingZeroCount(bishops);
            ulong bishop = 1ul << i;
            bishops &= bishops - 1;

            for (int direction = 0; direction < 4; direction++)
            {
                ulong[] ray = Magics.TargetedBishopAttacks[i, direction];
                for (int j = 0; j < ray.Length; j++)
                {
                    ulong cell = ray[j];
                    if ((cell & enemyPieces) != 0) moves.Add(new Move(bishop, cell));
                    if ((cell & occupied) != 0) break;
                }
            }
        }
    }

    private static void AddKingCaptures(List<Move> moves, ulong king, ulong enemyPieces)
    {
        bool up = (king & ~Chessboard.Rank8) != 0,
        down = (king & ~Chessboard.Rank1) != 0,
        right = (king & ~Chessboard.HFile) != 0,
        left = (king & ~Chessboard.AFile) != 0;

        // if can move up
        if (up && ((king >> 8) & enemyPieces) != 0)
            moves.Add(new Move(king, king >> 8));

        // if can move down
        if (down && ((king << 8) & enemyPieces) != 0)
            moves.Add(new Move(king, king << 8));

        // if can move right
        if (right && ((king >> 1) & enemyPieces) != 0)
            moves.Add(new Move(king, king >> 1));

        // if can move left
        if (left && ((king << 1) & enemyPieces) != 0)
            moves.Add(new Move(king, king << 1));

        // up right
        if (up && right && ((king >> 9) & enemyPieces) != 0)
            moves.Add(new Move(king, king >> 9));

        // up left
        if (up && left && ((king >> 7) & enemyPieces) != 0)
            moves.Add(new Move(king, king >> 7));

        // down right
        if (down && right && ((king << 7) & enemyPieces) != 0)
            moves.Add(new Move(king, king << 7));

        // down left
        if (down && left && ((king << 9) & enemyPieces) != 0)
            moves.Add(new Move(king, king << 9));
    }

    private static void AddPawnQuiescenceMoves(List<Move> moves, ulong pawns, IPositionState board, ulong enemyPieces)
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
                        moves.Add(new Move(pawn, pawn >> 7, false, false, false, PromotionType.Queen));
                        moves.Add(new Move(pawn, pawn >> 7, false, false, false, PromotionType.Rook));
                        moves.Add(new Move(pawn, pawn >> 7, false, false, false, PromotionType.Bishop));
                        moves.Add(new Move(pawn, pawn >> 7, false, false, false, PromotionType.Knight));
                    }
                    else
                    {
                        moves.Add(new Move(pawn, pawn >> 7));
                    }
                }

                // if the pawn can take to the right
                if (((pawn >> 9) & enemyPieces & ~Chessboard.AFile) != 0)
                {
                    // if moving forward will make it promote
                    if ((pawn & Chessboard.Rank7) != 0)
                    {
                        moves.Add(new Move(pawn, pawn >> 9, false, false, false, PromotionType.Queen));
                        moves.Add(new Move(pawn, pawn >> 9, false, false, false, PromotionType.Rook));
                        moves.Add(new Move(pawn, pawn >> 9, false, false, false, PromotionType.Bishop));
                        moves.Add(new Move(pawn, pawn >> 9, false, false, false, PromotionType.Knight));
                    }
                    else
                    {
                        moves.Add(new Move(pawn, pawn >> 9));
                    }
                }

                // if can take en passant to the left
                if (((pawn >> 7) & board.WhiteEnPassant & ~Chessboard.HFile) != 0)
                {
                    moves.Add(new Move(pawn, pawn >> 7, true, false, false, PromotionType.None));
                }

                // if can take en passant to the right
                if (((pawn >> 9) & board.WhiteEnPassant & ~Chessboard.AFile) != 0)
                {
                    moves.Add(new Move(pawn, pawn >> 9, true, false, false, PromotionType.None));
                }

                // if the pawn can move forward to promote
                if ((pawn & Chessboard.Rank7) != 0 && ((pawn >> 8) & ~occupiedBb) != 0)
                {
                    moves.Add(new Move(pawn, pawn >> 8, false, false, false, PromotionType.Queen));
                    moves.Add(new Move(pawn, pawn >> 8, false, false, false, PromotionType.Rook));
                    moves.Add(new Move(pawn, pawn >> 8, false, false, false, PromotionType.Bishop));
                    moves.Add(new Move(pawn, pawn >> 8, false, false, false, PromotionType.Knight));
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
                        moves.Add(new Move(pawn, pawn << 9, false, false, false, PromotionType.Queen));
                        moves.Add(new Move(pawn, pawn << 9, false, false, false, PromotionType.Rook));
                        moves.Add(new Move(pawn, pawn << 9, false, false, false, PromotionType.Bishop));
                        moves.Add(new Move(pawn, pawn << 9, false, false, false, PromotionType.Knight));
                    }
                    else
                    {
                        moves.Add(new Move(pawn, pawn << 9));
                    }
                }

                // if the pawn can take to the right
                if (((pawn << 7) & enemyPieces & ~Chessboard.AFile) != 0)
                {
                    // if moving forward will make it promote
                    if ((pawn & Chessboard.Rank2) != 0)
                    {
                        moves.Add(new Move(pawn, pawn << 7, false, false, false, PromotionType.Queen));
                        moves.Add(new Move(pawn, pawn << 7, false, false, false, PromotionType.Rook));
                        moves.Add(new Move(pawn, pawn << 7, false, false, false, PromotionType.Bishop));
                        moves.Add(new Move(pawn, pawn << 7, false, false, false, PromotionType.Knight));
                    }
                    else
                    {
                        moves.Add(new Move(pawn, pawn << 7));
                    }
                }

                // if can take en passant to the left
                if (((pawn << 9) & board.BlackEnPassant & ~Chessboard.HFile) != 0)
                {
                    moves.Add(new Move(pawn, pawn << 9, true, false, false, PromotionType.None));
                }

                // if can take en passant to the right
                if (((pawn << 7) & board.BlackEnPassant & ~Chessboard.AFile) != 0)
                {
                    moves.Add(new Move(pawn, pawn << 7, true, false, false, PromotionType.None));
                }

                // if the pawn can move forward to promote
                if ((pawn & Chessboard.Rank2) != 0 && ((pawn << 8) & ~occupiedBb) != 0)
                {
                    moves.Add(new Move(pawn, pawn << 8, false, false, false, PromotionType.Queen));
                    moves.Add(new Move(pawn, pawn << 8, false, false, false, PromotionType.Rook));
                    moves.Add(new Move(pawn, pawn << 8, false, false, false, PromotionType.Bishop));
                    moves.Add(new Move(pawn, pawn << 8, false, false, false, PromotionType.Knight));
                }
            }
        }
    }
}
