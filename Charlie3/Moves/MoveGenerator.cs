using Charlie.Board;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Charlie.Moves
{
    public class MoveGenerator
    {
        public IEnumerable<Move> GenerateLegalMoves(BoardState board) =>
            TrimIllegalMoves(GeneratePseudoLegalMoves(board), board);

        public IEnumerable<Move> GenerateQuiescenceMoves(BoardState board) =>
            TrimIllegalMoves(GeneratePseudoLegalQuiescenceMoves(board), board);

        public IEnumerable<Move> GenerateLegalMoves(BoardState board, IEnumerable<Move> bestMoves)
        {
            foreach (Move move in bestMoves)
                yield return move;

            var pseudoLegalMoves = GeneratePseudoLegalMoves(board);
            var legalMoves = TrimIllegalMoves(pseudoLegalMoves, board);

            foreach (Move move in legalMoves.Where(move => !bestMoves.Contains(move)))
                yield return move;
        }

        public IEnumerable<Move> TrimIllegalMoves(IEnumerable<Move> moves, BoardState board) =>
            moves.Where(m => !m.LeavesPlayerInCheck(board));

        public IEnumerable<Move> GeneratePseudoLegalMoves(BoardState board)
        {
            if (board.ToMove == PieceColour.White)
            {
                foreach (Move move in GeneratePawnMoves(board.BitBoard.WhitePawn, board))
                    yield return move;

                foreach (Move move in GenerateKnightCaptures(board.BitBoard.WhiteKnight, board.BitBoard.BlackPieces))
                    yield return move;

                foreach (Move move in GenerateBishopMoves(board.BitBoard.WhiteBishop, board.BitBoard.WhitePieces, board))
                    yield return move;

                foreach (Move move in GenerateQueenMoves(board.BitBoard.WhiteQueen, board.BitBoard.WhitePieces, board))
                    yield return move;

                foreach (Move move in GenerateKingMoves(board.BitBoard.WhiteKing, board.BitBoard.WhitePieces, board))
                    yield return move;

                foreach (Move move in GenerateRookMoves(board.BitBoard.WhiteRook, board.BitBoard.WhitePieces, board))
                    yield return move;

                foreach (Move move in GenerateKnightNonCaptures(board.BitBoard.WhiteKnight, board.BitBoard.Occupied))
                    yield return move;
            }
            else
            {
                foreach (Move move in GeneratePawnMoves(board.BitBoard.BlackPawn, board))
                    yield return move;

                foreach (Move move in GenerateKnightCaptures(board.BitBoard.BlackKnight, board.BitBoard.WhitePieces))
                    yield return move;

                foreach (Move move in GenerateBishopMoves(board.BitBoard.BlackBishop, board.BitBoard.BlackPieces, board))
                    yield return move;

                foreach (Move move in GenerateQueenMoves(board.BitBoard.BlackQueen, board.BitBoard.BlackPieces, board))
                    yield return move;

                foreach (Move move in GenerateKingMoves(board.BitBoard.BlackKing, board.BitBoard.BlackPieces, board))
                    yield return move;

                foreach (Move move in GenerateRookMoves(board.BitBoard.BlackRook, board.BitBoard.BlackPieces, board))
                    yield return move;

                foreach (Move move in GenerateKnightNonCaptures(board.BitBoard.BlackKnight, board.BitBoard.Occupied))
                    yield return move;
            }
        }

        public IEnumerable<Move> GeneratePseudoLegalQuiescenceMoves(BoardState board)
        {
            if (board.ToMove == PieceColour.White)
            {
                foreach (Move move in GeneratePawnQuiescenceMoves(board.BitBoard.WhitePawn, board))
                    yield return move;

                foreach (Move move in GenerateKnightCaptures(board.BitBoard.WhiteKnight, board.BitBoard.BlackPieces))
                    yield return move;

                foreach (Move move in GenerateBishopCaptures(board.BitBoard.WhiteBishop, board.BitBoard.BlackPieces, board))
                    yield return move;

                foreach (Move move in GenerateQueenCaptures(board.BitBoard.WhiteQueen, board.BitBoard.BlackPieces, board))
                    yield return move;

                foreach (Move move in GenerateKingCaptures(board.BitBoard.WhiteKing, board.BitBoard.BlackPieces))
                    yield return move;

                foreach (Move move in GenerateRookCaptures(board.BitBoard.WhiteRook, board.BitBoard.BlackPieces, board))
                    yield return move;
            }
            else
            {
                foreach (Move move in GeneratePawnQuiescenceMoves(board.BitBoard.BlackPawn, board))
                    yield return move;

                foreach (Move move in GenerateKnightCaptures(board.BitBoard.BlackKnight, board.BitBoard.WhitePieces))
                    yield return move;

                foreach (Move move in GenerateBishopCaptures(board.BitBoard.BlackBishop, board.BitBoard.WhitePieces, board))
                    yield return move;

                foreach (Move move in GenerateQueenCaptures(board.BitBoard.BlackQueen, board.BitBoard.WhitePieces, board))
                    yield return move;

                foreach (Move move in GenerateKingCaptures(board.BitBoard.BlackKing, board.BitBoard.WhitePieces))
                    yield return move;

                foreach (Move move in GenerateRookCaptures(board.BitBoard.BlackRook, board.BitBoard.WhitePieces, board))
                    yield return move;
            }
        }

        private IEnumerable<Move> GenerateKnightCaptures(ulong knights, ulong enemyPieces)
        {
            for (int i = 0; i < 64; i++)
            {
                ulong knight = knights & (1ul << i);
                if (knight == 0) continue;

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
        private IEnumerable<Move> GenerateKnightNonCaptures(ulong knights, ulong occupied)
        {
            for (int i = 0; i < 64; i++)
            {
                ulong knight = knights & (1ul << i);
                if (knight == 0) continue;

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

        private IEnumerable<Move> GenerateQueenMoves(ulong queens, ulong friendlyPieces, BoardState board)
        {
            foreach (Move move in GenerateBishopMoves(queens, friendlyPieces, board))
                yield return move;

            foreach (Move move in GenerateRookMoves(queens, friendlyPieces, board))
                yield return move;
        }

        private IEnumerable<Move> GenerateRookMoves(ulong rooks, ulong friendlyPieces, BoardState board)
        {
            for (int i = 0; i < 64; i++)
            {
                ulong rook = rooks & (1ul << i);
                if (rook == 0) continue;

                // scan up
                int distance = 0;
                while (((rook >> distance) & ~Chessboard.Rank8) != 0)
                {
                    distance += 8;
                    ulong newSq = rook >> distance;
                    if ((newSq & ~friendlyPieces) != 0) yield return new Move(rook, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan right
                distance = 0;
                while (((rook >> distance) & ~Chessboard.HFile) != 0)
                {
                    distance++;
                    ulong newSq = rook >> distance;
                    if ((newSq & ~friendlyPieces) != 0) yield return new Move(rook, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan left
                distance = 0;
                while (((rook << distance) & ~Chessboard.AFile) != 0)
                {
                    distance++;
                    ulong newSq = rook << distance;
                    if ((newSq & ~friendlyPieces) != 0) yield return new Move(rook, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan down
                distance = 0;
                while (((rook << distance) & ~Chessboard.Rank1) != 0)
                {
                    distance += 8;
                    ulong newSq = rook << distance;
                    if ((newSq & ~friendlyPieces) != 0) yield return new Move(rook, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }
            }
        }

        private IEnumerable<Move> GenerateBishopMoves(ulong bishops, ulong friendlyPieces, BoardState board)
        {
            for (int i = 0; i < 64; i++)
            {
                ulong bishop = bishops & (1ul << i);
                if (bishop == 0) continue;

                // scan up right
                int distance = 0;
                while (((bishop >> distance) & ~Chessboard.Rank8 & ~Chessboard.HFile) != 0)
                {
                    distance += 9;
                    ulong newSq = bishop >> distance;
                    if ((newSq & ~friendlyPieces) != 0) yield return new Move(bishop, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan up left
                distance = 0;
                while (((bishop >> distance) & ~Chessboard.Rank8 & ~Chessboard.AFile) != 0)
                {
                    distance += 7;
                    ulong newSq = bishop >> distance;
                    if ((newSq & ~friendlyPieces) != 0) yield return new Move(bishop, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan down right
                distance = 0;
                while (((bishop << distance) & ~Chessboard.Rank1 & ~Chessboard.HFile) != 0)
                {
                    distance += 7;
                    ulong newSq = bishop << distance;
                    if ((newSq & ~friendlyPieces) != 0) yield return new Move(bishop, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan down left
                distance = 0;
                while (((bishop << distance) & ~Chessboard.Rank1 & ~Chessboard.AFile) != 0)
                {
                    distance += 9;
                    ulong newSq = bishop << distance;
                    if ((newSq & ~friendlyPieces) != 0) yield return new Move(bishop, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }
            }
        }

        private IEnumerable<Move> GenerateKingMoves(ulong king, ulong friendlyPieces, BoardState board)
        {
            bool up = (king & ~Chessboard.Rank8) != 0,
            down = (king & ~Chessboard.Rank1) != 0,
            right = (king & ~Chessboard.HFile) != 0,
            left = (king & ~Chessboard.AFile) != 0;

            // if can move up
            if (up && ((king >> 8) & ~friendlyPieces) != 0)
                yield return new Move(king, king >> 8);

            // if can move down
            if (down && ((king << 8) & ~friendlyPieces) != 0)
                yield return new Move(king, king << 8);

            // if can move right
            if (right && ((king >> 1) & ~friendlyPieces) != 0)
                yield return new Move(king, king >> 1);

            // if can move left
            if (left && ((king << 1) & ~friendlyPieces) != 0)
                yield return new Move(king, king << 1);

            // up right
            if (up && right && ((king >> 9) & ~friendlyPieces) != 0)
                yield return new Move(king, king >> 9);

            // up left
            if (up && left && ((king >> 7) & ~friendlyPieces) != 0)
                yield return new Move(king, king >> 7);

            // down right
            if (down && right && ((king << 7) & ~friendlyPieces) != 0)
                yield return new Move(king, king << 7);

            // down left
            if (down && left && ((king << 9) & ~friendlyPieces) != 0)
                yield return new Move(king, king << 9);

            if (board.ToMove == PieceColour.White)
            {
                // If can short castle
                if ((board.CastleRules & 0b0001) != 0 &&
                    (board.BitBoard.Occupied & (Chessboard.SquareF1 | Chessboard.SquareG1)) == 0 &&
                    (board.BitBoard.WhiteRook & Chessboard.SquareH1) != 0 &&
                    !board.IsInCheck(PieceColour.White) &&
                    !board.IsUnderAttack(king >> 1, PieceColour.Black) &&
                    !board.IsUnderAttack(king >> 2, PieceColour.Black))
                {
                    yield return new Move(king, Chessboard.SquareG1, false, true, false, PromotionType.None);
                }

                // If can long castle
                if ((board.CastleRules & 0b0010) != 0 &&
                    (board.BitBoard.Occupied & (Chessboard.SquareB1 | Chessboard.SquareC1 | Chessboard.SquareD1)) == 0 &&
                    (board.BitBoard.WhiteRook & Chessboard.SquareA1) != 0 &&
                    !board.IsInCheck(PieceColour.White) &&
                    !board.IsUnderAttack(king << 1, PieceColour.Black) &&
                    !board.IsUnderAttack(king << 2, PieceColour.Black) &&
                    !board.IsUnderAttack(king << 3, PieceColour.Black))
                {
                    yield return new Move(king, Chessboard.SquareC1, false, true, false, PromotionType.None);
                }
            }
            else
            {
                // If can short castle
                if ((board.CastleRules & 0b0100) != 0 &&
                    (board.BitBoard.Occupied & (Chessboard.SquareF8 | Chessboard.SquareG8)) == 0 &&
                    (board.BitBoard.BlackRook & Chessboard.SquareH8) != 0 &&
                    !board.IsInCheck(PieceColour.Black) &&
                    !board.IsUnderAttack(king >> 1, PieceColour.White) &&
                    !board.IsUnderAttack(king >> 2, PieceColour.White))
                {
                    yield return new Move(king, Chessboard.SquareG8, false, true, false, PromotionType.None);
                }

                // If can long castle
                if ((board.CastleRules & 0b1000) != 0 &&
                    (board.BitBoard.Occupied & (Chessboard.SquareB8 | Chessboard.SquareC8 | Chessboard.SquareD8)) == 0 &&
                    (board.BitBoard.BlackRook & Chessboard.SquareA8) != 0 &&
                    !board.IsInCheck(PieceColour.Black) &&
                    !board.IsUnderAttack(king << 1, PieceColour.White) &&
                    !board.IsUnderAttack(king << 2, PieceColour.White) &&
                    !board.IsUnderAttack(king << 3, PieceColour.White))
                {
                    yield return new Move(king, Chessboard.SquareC8, false, true, false, PromotionType.None);
                }
            }
        }

        private IEnumerable<Move> GeneratePawnMoves(ulong pawns, BoardState board)
        {
            ulong occupiedBb = board.BitBoard.Occupied;
            ulong blackPiecesBb = board.BitBoard.BlackPieces;
            ulong whitePiecesBb = board.BitBoard.WhitePieces;

            for (int i = 0; i < 64; i++)
            {
                ulong b = 1ul << i;
                ulong pawn = pawns & b;
                if (pawn == 0) continue;

                if (board.ToMove == PieceColour.White)
                {
                    // if the pawn can take to the left
                    if (((pawn >> 7) & blackPiecesBb & ~Chessboard.HFile) != 0)
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
                    if (((pawn >> 9) & blackPiecesBb & ~Chessboard.AFile) != 0)
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

                    // if the pawn can move forward
                    if (((pawn >> 8) & ~occupiedBb) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & Chessboard.Rank7) != 0)
                        {
                            yield return new Move(pawn, pawn >> 8, false, false, false, PromotionType.Queen);
                            yield return new Move(pawn, pawn >> 8, false, false, false, PromotionType.Rook);
                            yield return new Move(pawn, pawn >> 8, false, false, false, PromotionType.Bishop);
                            yield return new Move(pawn, pawn >> 8, false, false, false, PromotionType.Knight);
                        }
                        else
                        {
                            yield return new Move(pawn, pawn >> 8);

                            // if the pawn can move a second space
                            if (((pawn >> 16) & Chessboard.Rank4 & ~occupiedBb) != 0)
                            {
                                yield return new Move(pawn, pawn >> 16, false, false, true, PromotionType.None);
                            }
                        }
                    }
                }
                else
                {
                    // if the pawn can take to the left
                    if (((pawn << 9) & whitePiecesBb & ~Chessboard.HFile) != 0)
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
                    if (((pawn << 7) & whitePiecesBb & ~Chessboard.AFile) != 0)
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

                    // if the pawn can move forward
                    if (((pawn << 8) & ~occupiedBb) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & Chessboard.Rank2) != 0)
                        {
                            yield return new Move(pawn, pawn << 8, false, false, false, PromotionType.Queen);
                            yield return new Move(pawn, pawn << 8, false, false, false, PromotionType.Rook);
                            yield return new Move(pawn, pawn << 8, false, false, false, PromotionType.Bishop);
                            yield return new Move(pawn, pawn << 8, false, false, false, PromotionType.Knight);
                        }
                        else
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
        }

        private IEnumerable<Move> GenerateQueenCaptures(ulong queens, ulong enemyPieces, BoardState board)
        {
            foreach (Move move in GenerateBishopCaptures(queens, enemyPieces, board))
                yield return move;

            foreach (Move move in GenerateRookCaptures(queens, enemyPieces, board))
                yield return move;
        }

        private IEnumerable<Move> GenerateRookCaptures(ulong rooks, ulong enemyPieces, BoardState board)
        {
            for (int i = 0; i < 64; i++)
            {
                ulong rook = rooks & (1ul << i);
                if (rook == 0) continue;

                // scan up
                int distance = 0;
                while (((rook >> distance) & ~Chessboard.Rank8) != 0)
                {
                    distance += 8;
                    ulong newSq = rook >> distance;
                    if ((newSq & enemyPieces) != 0) yield return new Move(rook, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan right
                distance = 0;
                while (((rook >> distance) & ~Chessboard.HFile) != 0)
                {
                    distance++;
                    ulong newSq = rook >> distance;
                    if ((newSq & enemyPieces) != 0) yield return new Move(rook, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan left
                distance = 0;
                while (((rook << distance) & ~Chessboard.AFile) != 0)
                {
                    distance++;
                    ulong newSq = rook << distance;
                    if ((newSq & enemyPieces) != 0) yield return new Move(rook, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan down
                distance = 0;
                while (((rook << distance) & ~Chessboard.Rank1) != 0)
                {
                    distance += 8;
                    ulong newSq = rook << distance;
                    if ((newSq & enemyPieces) != 0) yield return new Move(rook, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }
            }
        }

        private IEnumerable<Move> GenerateBishopCaptures(ulong bishops, ulong enemyPieces, BoardState board)
        {
            for (int i = 0; i < 64; i++)
            {
                ulong bishop = bishops & (1ul << i);
                if (bishop == 0) continue;

                // scan up right
                int distance = 0;
                while (((bishop >> distance) & ~Chessboard.Rank8 & ~Chessboard.HFile) != 0)
                {
                    distance += 9;
                    ulong newSq = bishop >> distance;
                    if ((newSq & enemyPieces) != 0) yield return new Move(bishop, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan up left
                distance = 0;
                while (((bishop >> distance) & ~Chessboard.Rank8 & ~Chessboard.AFile) != 0)
                {
                    distance += 7;
                    ulong newSq = bishop >> distance;
                    if ((newSq & enemyPieces) != 0) yield return new Move(bishop, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan down right
                distance = 0;
                while (((bishop << distance) & ~Chessboard.Rank1 & ~Chessboard.HFile) != 0)
                {
                    distance += 7;
                    ulong newSq = bishop << distance;
                    if ((newSq & enemyPieces) != 0) yield return new Move(bishop, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan down left
                distance = 0;
                while (((bishop << distance) & ~Chessboard.Rank1 & ~Chessboard.AFile) != 0)
                {
                    distance += 9;
                    ulong newSq = bishop << distance;
                    if ((newSq & enemyPieces) != 0) yield return new Move(bishop, newSq);
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }
            }
        }

        private IEnumerable<Move> GenerateKingCaptures(ulong king, ulong enemyPieces)
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

        private IEnumerable<Move> GeneratePawnQuiescenceMoves(ulong pawns, BoardState board)
        {
            ulong occupiedBb = board.BitBoard.Occupied;
            ulong blackPiecesBb = board.BitBoard.BlackPieces;
            ulong whitePiecesBb = board.BitBoard.WhitePieces;

            for (int i = 0; i < 64; i++)
            {
                ulong b = 1ul << i;
                ulong pawn = pawns & b;
                if (pawn == 0) continue;

                if (board.ToMove == PieceColour.White)
                {
                    // if the pawn can take to the left
                    if (((pawn >> 7) & blackPiecesBb & ~Chessboard.HFile) != 0)
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
                    if (((pawn >> 9) & blackPiecesBb & ~Chessboard.AFile) != 0)
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
                    if (((pawn << 9) & whitePiecesBb & ~Chessboard.HFile) != 0)
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
                    if (((pawn << 7) & whitePiecesBb & ~Chessboard.AFile) != 0)
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
}
