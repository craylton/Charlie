using Charlie3.Enums;
using System.Collections.Generic;

namespace Charlie3
{
    public class MoveGenerator
    {
        public List<Move> GeneratePseudoLegalMoves(BoardState board)
        {
            var moves = new List<Move>();

            if (board.ToMove == PieceColour.White)
            {
                moves.AddRange(GenerateQueenMoves(board.BitBoard.WhiteQueen, board.BitBoard.WhitePieces, board));
                moves.AddRange(GenerateBishopMoves(board.BitBoard.WhiteBishop, board.BitBoard.WhitePieces, board));
                moves.AddRange(GeneratePawnMoves(board.BitBoard.WhitePawn, board));
                moves.AddRange(GenerateKingMoves(board.BitBoard.WhiteKing, board.BitBoard.WhitePieces, board));
                moves.AddRange(GenerateRookMoves(board.BitBoard.WhiteRook, board.BitBoard.WhitePieces, board));
                moves.AddRange(GenerateKnightMoves(board.BitBoard.WhiteKnight, board.BitBoard.WhitePieces));
            }
            else
            {
                moves.AddRange(GenerateQueenMoves(board.BitBoard.BlackQueen, board.BitBoard.BlackPieces, board));
                moves.AddRange(GenerateBishopMoves(board.BitBoard.BlackBishop, board.BitBoard.BlackPieces, board));
                moves.AddRange(GeneratePawnMoves(board.BitBoard.BlackPawn, board));
                moves.AddRange(GenerateKingMoves(board.BitBoard.BlackKing, board.BitBoard.BlackPieces, board));
                moves.AddRange(GenerateRookMoves(board.BitBoard.BlackRook, board.BitBoard.BlackPieces, board));
                moves.AddRange(GenerateKnightMoves(board.BitBoard.BlackKnight, board.BitBoard.BlackPieces));
            }

            return moves;
        }

        public List<Move> GenerateLegalMoves(BoardState board) =>
            TrimIllegalMoves(GeneratePseudoLegalMoves(board), board);

        public List<Move> TrimIllegalMoves(List<Move> moves, BoardState board)
        {
            PieceColour attacker = board.ToMove == PieceColour.White ? PieceColour.Black : PieceColour.White;

            // If there is a chance we are in check, do a more comprehensive search
            moves.RemoveAll(m =>
            {
                BoardState newState = board.MakeMove(m);
                // Look if there are any enemy pieces aimed at the king
                if (newState.IsInPseudoCheck(attacker))
                    return newState.IsInCheck(board.ToMove);

                return false;
            });

            return moves;
        }

        private IEnumerable<Move> GenerateKnightMoves(ulong knights, ulong friendlyPieces)
        {
            var moves = new List<Move>();

            for (int i = 0; i < 64; i++)
            {
                var knight = knights & (1ul << i);
                if (knight == 0) continue;

                bool up = (knight & ~ChessBoard.Rank8) != 0,
                down = (knight & ~ChessBoard.Rank1) != 0,
                right = (knight & ~ChessBoard.HFile) != 0,
                left = (knight & ~ChessBoard.AFile) != 0,
                up2 = (knight & ~(ChessBoard.Rank7 | ChessBoard.Rank8)) != 0,
                down2 = (knight & ~(ChessBoard.Rank1 | ChessBoard.Rank2)) != 0,
                right2 = (knight & ~(ChessBoard.GFile | ChessBoard.HFile)) != 0,
                left2 = (knight & ~(ChessBoard.AFile | ChessBoard.BFile)) != 0;

                if (up2 && right && ((knight >> 17) & ~friendlyPieces) != 0) moves.Add(new Move(knight, knight >> 17));
                if (up && right2 && ((knight >> 10) & ~friendlyPieces) != 0) moves.Add(new Move(knight, knight >> 10));
                if (down && right2 && ((knight << 6) & ~friendlyPieces) != 0) moves.Add(new Move(knight, knight << 6));
                if (down2 && right && ((knight << 15) & ~friendlyPieces) != 0) moves.Add(new Move(knight, knight << 15));
                if (down2 && left && ((knight << 17) & ~friendlyPieces) != 0) moves.Add(new Move(knight, knight << 17));
                if (down && left2 && ((knight << 10) & ~friendlyPieces) != 0) moves.Add(new Move(knight, knight << 10));
                if (up && left2 && ((knight >> 6) & ~friendlyPieces) != 0) moves.Add(new Move(knight, knight >> 6));
                if (up2 && left && ((knight >> 15) & ~friendlyPieces) != 0) moves.Add(new Move(knight, knight >> 15));
            }

            return moves;
        }

        private IEnumerable<Move> GenerateQueenMoves(ulong queens, ulong friendlyPieces, BoardState board)
        {
            var moves = new List<Move>();

            moves.AddRange(GenerateBishopMoves(queens, friendlyPieces, board));
            moves.AddRange(GenerateRookMoves(queens, friendlyPieces, board));

            return moves;
        }

        private IEnumerable<Move> GenerateRookMoves(ulong rooks, ulong friendlyPieces, BoardState board)
        {
            var moves = new List<Move>();

            for (int i = 0; i < 64; i++)
            {
                var rook = rooks & (1ul << i);
                if (rook == 0) continue;

                // scan up
                int distance = 0;
                while (((rook >> distance) & ~ChessBoard.Rank8) != 0)
                {
                    distance += 8;
                    var newSq = rook >> distance;
                    if ((newSq & ~friendlyPieces) != 0) moves.Add(new Move(rook, newSq));
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan down
                distance = 0;
                while (((rook << distance) & ~ChessBoard.Rank1) != 0)
                {
                    distance += 8;
                    var newSq = rook << distance;
                    if ((newSq & ~friendlyPieces) != 0) moves.Add(new Move(rook, newSq));
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan right
                distance = 0;
                while (((rook >> distance) & ~ChessBoard.HFile) != 0)
                {
                    distance++;
                    var newSq = rook >> distance;
                    if ((newSq & ~friendlyPieces) != 0) moves.Add(new Move(rook, newSq));
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan left
                distance = 0;
                while (((rook << distance) & ~ChessBoard.AFile) != 0)
                {
                    distance++;
                    var newSq = rook << distance;
                    if ((newSq & ~friendlyPieces) != 0) moves.Add(new Move(rook, newSq));
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }
            }

            return moves;
        }

        private IEnumerable<Move> GenerateBishopMoves(ulong bishops, ulong friendlyPieces, BoardState board)
        {
            var moves = new List<Move>();

            for (int i = 0; i < 64; i++)
            {
                var bishop = bishops & (1ul << i);
                if (bishop == 0) continue;

                // scan up right
                int distance = 0;
                while (((bishop >> distance) & ~ChessBoard.Rank8 & ~ChessBoard.HFile) != 0)
                {
                    distance += 9;
                    var newSq = bishop >> distance;
                    if ((newSq & ~friendlyPieces) != 0) moves.Add(new Move(bishop, newSq));
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan up left
                distance = 0;
                while (((bishop >> distance) & ~ChessBoard.Rank8 & ~ChessBoard.AFile) != 0)
                {
                    distance += 7;
                    var newSq = bishop >> distance;
                    if ((newSq & ~friendlyPieces) != 0) moves.Add(new Move(bishop, newSq));
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan down right
                distance = 0;
                while (((bishop << distance) & ~ChessBoard.Rank1 & ~ChessBoard.HFile) != 0)
                {
                    distance += 7;
                    var newSq = bishop << distance;
                    if ((newSq & ~friendlyPieces) != 0) moves.Add(new Move(bishop, newSq));
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }

                // scan down left
                distance = 0;
                while (((bishop << distance) & ~ChessBoard.Rank1 & ~ChessBoard.AFile) != 0)
                {
                    distance += 9;
                    var newSq = bishop << distance;
                    if ((newSq & ~friendlyPieces) != 0) moves.Add(new Move(bishop, newSq));
                    if ((newSq & board.BitBoard.Occupied) != 0) break;
                }
            }

            return moves;
        }

        private IEnumerable<Move> GenerateKingMoves(ulong king, ulong friendlyPieces, BoardState board)
        {
            var moves = new List<Move>();

            bool up = (king & ~ChessBoard.Rank8) != 0,
            down = (king & ~ChessBoard.Rank1) != 0,
            right = (king & ~ChessBoard.HFile) != 0,
            left = (king & ~ChessBoard.AFile) != 0;

            // if can move up
            if (up && ((king >> 8) & ~friendlyPieces) != 0)
                moves.Add(new Move(king, king >> 8));

            // if can move down
            if (down && ((king << 8) & ~friendlyPieces) != 0)
                moves.Add(new Move(king, king << 8));

            // if can move right
            if (right && ((king >> 1) & ~friendlyPieces) != 0)
                moves.Add(new Move(king, king >> 1));

            // if can move left
            if (left && ((king << 1) & ~friendlyPieces) != 0)
                moves.Add(new Move(king, king << 1));

            // up right
            if (up && right && ((king >> 9) & ~friendlyPieces) != 0)
                moves.Add(new Move(king, king >> 9));

            // up left
            if (up && left && ((king >> 7) & ~friendlyPieces) != 0)
                moves.Add(new Move(king, king >> 7));

            // down right
            if (down && right && ((king << 7) & ~friendlyPieces) != 0)
                moves.Add(new Move(king, king << 7));

            // down left
            if (down && left && ((king << 9) & ~friendlyPieces) != 0)
                moves.Add(new Move(king, king << 9));

            if (board.ToMove == PieceColour.White)
            {
                // If can short castle
                if ((board.CastleRules & 0b0001) != 0 &&
                    (board.BitBoard.Occupied & (ChessBoard.SquareF1 | ChessBoard.SquareG1)) == 0 &&
                    (board.BitBoard.WhiteRook & ChessBoard.SquareH1) != 0 &&
                    !board.IsInCheck(PieceColour.White) &&
                    !board.IsUnderAttack(king >> 1, PieceColour.Black) &&
                    !board.IsUnderAttack(king >> 2, PieceColour.Black))
                {
                    moves.Add(new Move(king, ChessBoard.SquareG1, false, true, false, PromotionType.None));
                }

                // If can long castle
                if ((board.CastleRules & 0b0010) != 0 &&
                    (board.BitBoard.Occupied & (ChessBoard.SquareB1 | ChessBoard.SquareC1 | ChessBoard.SquareD1)) == 0 &&
                    (board.BitBoard.WhiteRook & ChessBoard.SquareA1) != 0 &&
                    !board.IsInCheck(PieceColour.White) &&
                    !board.IsUnderAttack(king << 1, PieceColour.Black) &&
                    !board.IsUnderAttack(king << 2, PieceColour.Black) &&
                    !board.IsUnderAttack(king << 3, PieceColour.Black))
                {
                    moves.Add(new Move(king, ChessBoard.SquareC1, false, true, false, PromotionType.None));
                }
            }
            else
            {
                // If can short castle
                if ((board.CastleRules & 0b0100) != 0 &&
                    (board.BitBoard.Occupied & (ChessBoard.SquareF8 | ChessBoard.SquareG8)) == 0 &&
                    (board.BitBoard.BlackRook & ChessBoard.SquareH8) != 0 &&
                    !board.IsInCheck(PieceColour.Black) &&
                    !board.IsUnderAttack(king >> 1, PieceColour.White) &&
                    !board.IsUnderAttack(king >> 2, PieceColour.White))
                {
                    moves.Add(new Move(king, ChessBoard.SquareG8, false, true, false, PromotionType.None));
                }

                // If can long castle
                if ((board.CastleRules & 0b1000) != 0 &&
                    (board.BitBoard.Occupied & (ChessBoard.SquareB8 | ChessBoard.SquareC8 | ChessBoard.SquareD8)) == 0 &&
                    (board.BitBoard.BlackRook & ChessBoard.SquareA8) != 0 &&
                    !board.IsInCheck(PieceColour.Black) &&
                    !board.IsUnderAttack(king << 1, PieceColour.White) &&
                    !board.IsUnderAttack(king << 2, PieceColour.White) &&
                    !board.IsUnderAttack(king << 3, PieceColour.White))
                {
                    moves.Add(new Move(king, ChessBoard.SquareC8, false, true, false, PromotionType.None));
                }
            }

            return moves;
        }

        private IEnumerable<Move> GeneratePawnMoves(ulong pawns, BoardState board)
        {
            var moves = new List<Move>();

            var occupiedBb = board.BitBoard.Occupied;
            var blackPiecesBb = board.BitBoard.BlackPieces;
            var whitePiecesBb = board.BitBoard.WhitePieces;

            for (int i = 0; i < 64; i++)
            {
                ulong b = (ulong)1 << i;
                var pawn = pawns & b;

                if (board.ToMove == PieceColour.White)
                {
                    // if the pawn can move forward
                    if (((pawn >> 8) & ~occupiedBb) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & ChessBoard.Rank7) != 0)
                        {
                            moves.Add(new Move(pawn, pawn >> 8, false, false, false, PromotionType.Queen));
                            moves.Add(new Move(pawn, pawn >> 8, false, false, false, PromotionType.Rook));
                            moves.Add(new Move(pawn, pawn >> 8, false, false, false, PromotionType.Bishop));
                            moves.Add(new Move(pawn, pawn >> 8, false, false, false, PromotionType.Knight));
                        }
                        else
                        {
                            moves.Add(new Move(pawn, pawn >> 8));

                            // if the pawn can move a second space
                            if (((pawn >> 16) & ChessBoard.Rank4 & ~occupiedBb) != 0)
                            {
                                moves.Add(new Move(pawn, pawn >> 16, false, false, true, PromotionType.None));
                            }
                        }
                    }

                    // if the pawn can take to the left
                    if (((pawn >> 7) & blackPiecesBb & ~ChessBoard.HFile) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & ChessBoard.Rank7) != 0)
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
                    if (((pawn >> 9) & blackPiecesBb & ~ChessBoard.AFile) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & ChessBoard.Rank7) != 0)
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
                    if (((pawn >> 7) & board.WhiteEnPassant & ~ChessBoard.HFile) != 0)
                    {
                        moves.Add(new Move(pawn, pawn >> 7, true, false, false, PromotionType.None));
                    }

                    // if can take en passant to the right
                    if (((pawn >> 9) & board.WhiteEnPassant & ~ChessBoard.AFile) != 0)
                    {
                        moves.Add(new Move(pawn, pawn >> 9, true, false, false, PromotionType.None));
                    }
                }
                else
                {
                    // if the pawn can move forward
                    if (((pawn << 8) & ~occupiedBb) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & ChessBoard.Rank2) != 0)
                        {
                            moves.Add(new Move(pawn, pawn << 8, false, false, false, PromotionType.Queen));
                            moves.Add(new Move(pawn, pawn << 8, false, false, false, PromotionType.Rook));
                            moves.Add(new Move(pawn, pawn << 8, false, false, false, PromotionType.Bishop));
                            moves.Add(new Move(pawn, pawn << 8, false, false, false, PromotionType.Knight));
                        }
                        else
                        {
                            moves.Add(new Move(pawn, pawn << 8));

                            // if the pawn can move a second space
                            if (((pawn << 16) & ChessBoard.Rank5 & ~occupiedBb) != 0)
                            {
                                moves.Add(new Move(pawn, pawn << 16, false, false, true, PromotionType.None));
                            }
                        }
                    }

                    // if the pawn can take to the left
                    if (((pawn << 9) & whitePiecesBb & ~ChessBoard.HFile) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & ChessBoard.Rank2) != 0)
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
                    if (((pawn << 7) & whitePiecesBb & ~ChessBoard.AFile) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & ChessBoard.Rank2) != 0)
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
                    if (((pawn << 9) & board.BlackEnPassant & ~ChessBoard.HFile) != 0)
                    {
                        moves.Add(new Move(pawn, pawn << 9, true, false, false, PromotionType.None));
                    }

                    // if can take en passant to the right
                    if (((pawn << 7) & board.BlackEnPassant & ~ChessBoard.AFile) != 0)
                    {
                        moves.Add(new Move(pawn, pawn << 7, true, false, false, PromotionType.None));
                    }
                }
            }

            return moves;
        }
    }
}
