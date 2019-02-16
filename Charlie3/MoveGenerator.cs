using System.Collections.Generic;

namespace Charlie3
{
    public class MoveGenerator
    {
        public IEnumerable<Move> GenerateLegalMoves(BoardState board)
        {
            var moves = new List<Move>();

            moves.AddRange(GeneratePawnMoves(board.BitBoard.WhitePawn, board));

            if (board.ToMove == PieceColour.White)
            {
                // Generate knight moves

                // Generate bishop moves

                // Generate rook moves

                // Generate queen moves

                // Generate king moves

                // Filter out moves that leave us in check
            }
            else
            {
                // Generate knight moves

                // Generate bishop moves

                // Generate rook moves

                // Generate queen moves

                // Generate king moves

                // Filter out moves that leave us in check
            }

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
                            moves.Add(new Move(pawn, pawn >> 8, false, false, PromotionType.Queen));
                            moves.Add(new Move(pawn, pawn >> 8, false, false, PromotionType.Rook));
                            moves.Add(new Move(pawn, pawn >> 8, false, false, PromotionType.Bishop));
                            moves.Add(new Move(pawn, pawn >> 8, false, false, PromotionType.Knight));
                        }
                        else
                        {
                            moves.Add(new Move(pawn, pawn >> 8, false, false, PromotionType.None));

                            // if the pawn can move a second space
                            if (((pawn >> 16) & 0x00_00_00_FF_00_00_00_00 & ~board.BitBoard.Occupied) != 0)
                            {
                                moves.Add(new Move(pawn, pawn >> 16, false, false, PromotionType.None));
                            }
                        }
                    }

                    // if the pawn can take to the left
                    if (((pawn >> 7) & board.BitBoard.BlackPieces & ~0x01_01_01_01_01_01_01_01ul) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & 0x00_00_00_00_00_00_FF_00) != 0)
                        {
                            moves.Add(new Move(pawn, pawn >> 7, false, false, PromotionType.Queen));
                            moves.Add(new Move(pawn, pawn >> 7, false, false, PromotionType.Rook));
                            moves.Add(new Move(pawn, pawn >> 7, false, false, PromotionType.Bishop));
                            moves.Add(new Move(pawn, pawn >> 7, false, false, PromotionType.Knight));
                        }
                        else
                        {
                            moves.Add(new Move(pawn, pawn >> 7, false, false, PromotionType.None));
                        }
                    }

                    // if the pawn can take to the right
                    if (((pawn >> 9) & board.BitBoard.BlackPieces & ~0x80_80_80_80_80_80_80_80ul) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & 0x00_00_00_00_00_00_FF_00) != 0)
                        {
                            moves.Add(new Move(pawn, pawn >> 9, false, false, PromotionType.Queen));
                            moves.Add(new Move(pawn, pawn >> 9, false, false, PromotionType.Rook));
                            moves.Add(new Move(pawn, pawn >> 9, false, false, PromotionType.Bishop));
                            moves.Add(new Move(pawn, pawn >> 9, false, false, PromotionType.Knight));
                        }
                        else
                        {
                            moves.Add(new Move(pawn, pawn >> 9, false, false, PromotionType.None));
                        }
                    }

                    // if can take en passant to the left
                    if (((pawn >> 7) & board.WhiteEnPassant) != 0)
                    {
                        moves.Add(new Move(pawn, pawn >> 7, true, false, PromotionType.None));
                    }

                    // if can take en passant to the right
                    if (((pawn >> 9) & board.WhiteEnPassant) != 0)
                    {
                        moves.Add(new Move(pawn, pawn >> 9, true, false, PromotionType.None));
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
                            moves.Add(new Move(pawn, pawn << 8, false, false, PromotionType.Queen));
                            moves.Add(new Move(pawn, pawn << 8, false, false, PromotionType.Rook));
                            moves.Add(new Move(pawn, pawn << 8, false, false, PromotionType.Bishop));
                            moves.Add(new Move(pawn, pawn << 8, false, false, PromotionType.Knight));
                        }
                        else
                        {
                            moves.Add(new Move(pawn, pawn << 8, false, false, PromotionType.None));

                            // if the pawn can move a second space
                            if (((pawn << 16) & 0x00_00_00_FF_00_00_00_00 & ~board.BitBoard.Occupied) != 0)
                            {
                                moves.Add(new Move(pawn, pawn << 16, false, false, PromotionType.None));
                            }
                        }
                    }

                    // if the pawn can take to the left
                    if (((pawn << 9) & board.BitBoard.WhitePieces & ~0x01_01_01_01_01_01_01_01ul) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & 0x00_FF_00_00_00_00_00_00) != 0)
                        {
                            moves.Add(new Move(pawn, pawn << 9, false, false, PromotionType.Queen));
                            moves.Add(new Move(pawn, pawn << 9, false, false, PromotionType.Rook));
                            moves.Add(new Move(pawn, pawn << 9, false, false, PromotionType.Bishop));
                            moves.Add(new Move(pawn, pawn << 9, false, false, PromotionType.Knight));
                        }
                        else
                        {
                            moves.Add(new Move(pawn, pawn << 9, false, false, PromotionType.None));
                        }
                    }

                    // if the pawn can take to the right
                    if (((pawn >> 7) & board.BitBoard.WhitePieces & ~0x80_80_80_80_80_80_80_80ul) != 0)
                    {
                        // if moving forward will make it promote
                        if ((pawn & 0x00_FF_00_00_00_00_00_00) != 0)
                        {
                            moves.Add(new Move(pawn, pawn << 7, false, false, PromotionType.Queen));
                            moves.Add(new Move(pawn, pawn << 7, false, false, PromotionType.Rook));
                            moves.Add(new Move(pawn, pawn << 7, false, false, PromotionType.Bishop));
                            moves.Add(new Move(pawn, pawn << 7, false, false, PromotionType.Knight));
                        }
                        else
                        {
                            moves.Add(new Move(pawn, pawn << 7, false, false, PromotionType.None));
                        }
                    }

                    // if can take en passant to the left
                    if (((pawn << 9) & board.BlackEnPassant) != 0)
                    {
                        moves.Add(new Move(pawn, pawn << 9, true, false, PromotionType.None));
                    }

                    // if can take en passant to the right
                    if (((pawn << 7) & board.BlackEnPassant) != 0)
                    {
                        moves.Add(new Move(pawn, pawn << 7, true, false, PromotionType.None));
                    }
                }
            }

            return moves;
        }
    }
}
