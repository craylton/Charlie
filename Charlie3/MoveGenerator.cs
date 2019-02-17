using System.Collections.Generic;

namespace Charlie3
{
    public class MoveGenerator
    {
        public IEnumerable<Move> GenerateLegalMoves(BoardState board)
        {
            var moves = new List<Move>();

            if (board.ToMove == PieceColour.White)
            {
                moves.AddRange(GeneratePawnMoves(board.BitBoard.WhitePawn, board));
                // Generate knight moves
                // Generate bishop moves
                // Generate rook moves
                // Generate queen moves
                moves.AddRange(GenerateKingMoves(board.BitBoard.WhiteKing, board.BitBoard.WhitePieces, board));
            }
            else
            {
                moves.AddRange(GeneratePawnMoves(board.BitBoard.BlackPawn, board));
                // Generate knight moves
                // Generate bishop moves
                // Generate rook moves
                // Generate queen moves
                moves.AddRange(GenerateKingMoves(board.BitBoard.BlackKing, board.BitBoard.BlackPieces, board));
            }

            // Remove any moves that leave the king in check
            moves.RemoveAll(m => board.MakeMove(m).IsInCheck(board.ToMove));

            return moves;
        }

        private IEnumerable<Move> GenerateKingMoves(ulong king, ulong friendlyPieces, BoardState board)
        {
            List<Move> moves = new List<Move>();

            bool up = (king & ~0x00_00_00_00_00_00_00_FFul) != 0,
            down = (king & ~0xFF_00_00_00_00_00_00_00ul) != 0,
            right = (king & ~0x01_01_01_01_01_01_01_01ul) != 0,
            left = (king & ~0x80_80_80_80_80_80_80_80ul) != 0;

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

            return moves;
        }

        private IEnumerable<Move> GeneratePawnMoves(ulong pawns, BoardState board)
        {
            List<Move> moves = new List<Move>();

            for (int i = 0; i < 64; i++)
            {
                ulong b = (ulong)1 << i;
                var pawn = pawns & b;

                if (board.ToMove == PieceColour.White)
                {
                    // if the pawn can move forward
                    if (((pawn >> 8) & ~board.BitBoard.Occupied) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & 0x00_00_00_00_00_00_FF_00) != 0)
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
                            if (((pawn >> 16) & 0x00_00_00_FF_00_00_00_00 & ~board.BitBoard.Occupied) != 0)
                            {
                                moves.Add(new Move(pawn, pawn >> 16, false, false, true, PromotionType.None));
                            }
                        }
                    }

                    // if the pawn can take to the left
                    if (((pawn >> 7) & board.BitBoard.BlackPieces & ~0x01_01_01_01_01_01_01_01ul) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & 0x00_00_00_00_00_00_FF_00) != 0)
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
                    if (((pawn >> 9) & board.BitBoard.BlackPieces & ~0x80_80_80_80_80_80_80_80ul) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & 0x00_00_00_00_00_00_FF_00) != 0)
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
                    if (((pawn >> 7) & board.WhiteEnPassant & ~0x01_01_01_01_01_01_01_01ul) != 0)
                    {
                        moves.Add(new Move(pawn, pawn >> 7, true, false, false, PromotionType.None));
                    }

                    // if can take en passant to the right
                    if (((pawn >> 9) & board.WhiteEnPassant & ~0x80_80_80_80_80_80_80_80ul) != 0)
                    {
                        moves.Add(new Move(pawn, pawn >> 9, true, false, false, PromotionType.None));
                    }
                }
                else
                {
                    // if the pawn can move forward
                    if (((pawn << 8) & ~board.BitBoard.Occupied) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & 0x00_FF_00_00_00_00_00_00) != 0)
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
                            if (((pawn << 16) & 0x00_00_00_00_FF_00_00_00 & ~board.BitBoard.Occupied) != 0)
                            {
                                moves.Add(new Move(pawn, pawn << 16, false, false, true, PromotionType.None));
                            }
                        }
                    }

                    // if the pawn can take to the left
                    if (((pawn << 9) & board.BitBoard.WhitePieces & ~0x01_01_01_01_01_01_01_01ul) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & 0x00_FF_00_00_00_00_00_00) != 0)
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
                    if (((pawn << 7) & board.BitBoard.WhitePieces & ~0x80_80_80_80_80_80_80_80ul) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & 0x00_FF_00_00_00_00_00_00) != 0)
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
                    if (((pawn << 9) & board.BlackEnPassant & ~0x01_01_01_01_01_01_01_01ul) != 0)
                    {
                        moves.Add(new Move(pawn, pawn << 9, true, false, false, PromotionType.None));
                    }

                    // if can take en passant to the right
                    if (((pawn << 7) & board.BlackEnPassant & ~0x80_80_80_80_80_80_80_80ul) != 0)
                    {
                        moves.Add(new Move(pawn, pawn << 7, true, false, false, PromotionType.None));
                    }
                }
            }

            return moves;
        }
    }
}
