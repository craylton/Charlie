﻿using Charlie3.Enums;

namespace Charlie3
{
    public readonly struct BitBoard
    {
        private const ulong DefaultWhiteKing = ChessBoard.SquareE1;
        private const ulong DefaultBlackKing = ChessBoard.SquareE8;
        private const ulong DefaultWhiteQueen = ChessBoard.SquareD1;
        private const ulong DefaultBlackQueen = ChessBoard.SquareD8;
        private const ulong DefaultWhiteRook = ChessBoard.SquareA1 | ChessBoard.SquareH1;
        private const ulong DefaultBlackRook = ChessBoard.SquareA8 | ChessBoard.SquareH8;
        private const ulong DefaultWhiteBishop = ChessBoard.SquareC1 | ChessBoard.SquareF1;
        private const ulong DefaultBlackBishop = ChessBoard.SquareC8 | ChessBoard.SquareF8;
        private const ulong DefaultWhiteKnight = ChessBoard.SquareB1 | ChessBoard.SquareG1;
        private const ulong DefaultBlackKnight = ChessBoard.SquareB8 | ChessBoard.SquareG8;
        private const ulong DefaultWhitePawn = ChessBoard.Rank2;
        private const ulong DefaultBlackPawn = ChessBoard.Rank7;

        private BitBoard(ulong[] kings, ulong[] queens, ulong[] rooks, ulong[] bishops, ulong[] knights, ulong[] pawns)
        {
            WhiteKing = kings[0];
            BlackKing = kings[1];
            WhiteQueen = queens[0];
            BlackQueen = queens[1];
            WhiteRook = rooks[0];
            BlackRook = rooks[1];
            WhiteBishop = bishops[0];
            BlackBishop = bishops[1];
            WhiteKnight = knights[0];
            BlackKnight = knights[1];
            WhitePawn = pawns[0];
            BlackPawn = pawns[1];
        }

        public BitBoard(BitBoard oldBb, Move move)
        {
            WhiteKing = oldBb.WhiteKing;
            BlackKing = oldBb.BlackKing;
            WhiteQueen = oldBb.WhiteQueen;
            BlackQueen = oldBb.BlackQueen;
            WhiteRook = oldBb.WhiteRook;
            BlackRook = oldBb.BlackRook;
            WhiteBishop = oldBb.WhiteBishop;
            BlackBishop = oldBb.BlackBishop;
            WhiteKnight = oldBb.WhiteKnight;
            BlackKnight = oldBb.BlackKnight;
            WhitePawn = oldBb.WhitePawn;
            BlackPawn = oldBb.BlackPawn;

            // If castle, we need to move both the king and the rook
            if (move.IsCastle)
            {
                // White long castle
                if (move.ToCell == ChessBoard.SquareC1)
                {
                    WhiteKing = move.ToCell;
                    WhiteRook &= ~ChessBoard.SquareA1;
                    WhiteRook |= ChessBoard.SquareD1;
                }
                // White short castle
                else if (move.ToCell == ChessBoard.SquareG1)
                {
                    WhiteKing = move.ToCell;
                    WhiteRook &= ~ChessBoard.SquareH1;
                    WhiteRook |= ChessBoard.SquareF1;
                }
                // Black long castle
                else if (move.ToCell == ChessBoard.SquareC8)
                {
                    BlackKing = move.ToCell;
                    BlackRook &= ~ChessBoard.SquareA8;
                    BlackRook |= ChessBoard.SquareD8;
                }
                // Black short castle
                else if (move.ToCell == ChessBoard.SquareG8)
                {
                    BlackKing = move.ToCell;
                    BlackRook &= ~ChessBoard.SquareH8;
                    BlackRook |= ChessBoard.SquareF8;
                }

                return;
            }

            var squaresToKeep = ~move.FromCell & ~move.ToCell;

            // Remove all pieces from the 'From' square and the 'To' square
            WhiteKing &= squaresToKeep;
            BlackKing &= squaresToKeep;
            WhiteQueen &= squaresToKeep;
            BlackQueen &= squaresToKeep;
            WhiteRook &= squaresToKeep;
            BlackRook &= squaresToKeep;
            WhiteBishop &= squaresToKeep;
            BlackBishop &= squaresToKeep;
            WhiteKnight &= squaresToKeep;
            BlackKnight &= squaresToKeep;
            WhitePawn &= squaresToKeep;
            BlackPawn &= squaresToKeep;

            // If en-passant, remove the captured pawn and move the capturing pawn
            if (move.IsEnPassant)
            {
                // If white captured en passant
                if ((oldBb.WhitePawn & move.FromCell) != 0)
                {
                    WhitePawn |= move.ToCell;
                    BlackPawn &= ~(move.ToCell << 8);
                }
                // If black captured en passant
                else
                {
                    BlackPawn |= move.ToCell;
                    WhitePawn &= ~(move.ToCell >> 8);
                }

                return;
            }

            // If promotion, we need to make sure the piece on the new square is the correct type
            if (move.PromotionType != PromotionType.None)
            {
                bool whitePromoted = (move.ToCell & ChessBoard.Rank8) != 0;
                switch (move.PromotionType)
                {
                    case PromotionType.Queen when whitePromoted:
                        WhiteQueen |= move.ToCell;
                        break;
                    case PromotionType.Queen:
                        BlackQueen |= move.ToCell;
                        break;
                    case PromotionType.Rook when whitePromoted:
                        WhiteRook |= move.ToCell;
                        break;
                    case PromotionType.Rook:
                        BlackRook |= move.ToCell;
                        break;
                    case PromotionType.Bishop when whitePromoted:
                        WhiteBishop |= move.ToCell;
                        break;
                    case PromotionType.Bishop:
                        BlackBishop |= move.ToCell;
                        break;
                    case PromotionType.Knight when whitePromoted:
                        WhiteKnight |= move.ToCell;
                        break;
                    case PromotionType.Knight:
                        BlackKnight |= move.ToCell;
                        break;
                }

                return;
            }

            // Find out which piece moved, and add one to the 'To' square
            if ((oldBb.WhiteKing & move.FromCell) != 0) WhiteKing |= move.ToCell;
            if ((oldBb.BlackKing & move.FromCell) != 0) BlackKing |= move.ToCell;
            if ((oldBb.WhiteQueen & move.FromCell) != 0) WhiteQueen |= move.ToCell;
            if ((oldBb.BlackQueen & move.FromCell) != 0) BlackQueen |= move.ToCell;
            if ((oldBb.WhiteRook & move.FromCell) != 0) WhiteRook |= move.ToCell;
            if ((oldBb.BlackRook & move.FromCell) != 0) BlackRook |= move.ToCell;
            if ((oldBb.WhiteBishop & move.FromCell) != 0) WhiteBishop |= move.ToCell;
            if ((oldBb.BlackBishop & move.FromCell) != 0) BlackBishop |= move.ToCell;
            if ((oldBb.WhiteKnight & move.FromCell) != 0) WhiteKnight |= move.ToCell;
            if ((oldBb.BlackKnight & move.FromCell) != 0) BlackKnight |= move.ToCell;
            if ((oldBb.WhitePawn & move.FromCell) != 0) WhitePawn |= move.ToCell;
            if ((oldBb.BlackPawn & move.FromCell) != 0) BlackPawn |= move.ToCell;
        }

        public static BitBoard GetDefault() => new BitBoard(
            new[] { DefaultWhiteKing, DefaultBlackKing },
            new[] { DefaultWhiteQueen, DefaultBlackQueen },
            new[] { DefaultWhiteRook, DefaultBlackRook },
            new[] { DefaultWhiteBishop, DefaultBlackBishop },
            new[] { DefaultWhiteKnight, DefaultBlackKnight },
            new[] { DefaultWhitePawn, DefaultBlackPawn });

        public ulong WhiteKing { get; }
        public ulong BlackKing { get; }

        public ulong WhiteQueen { get; }
        public ulong BlackQueen { get; }

        public ulong WhiteRook { get; }
        public ulong BlackRook { get; }

        public ulong WhiteBishop { get; }
        public ulong BlackBishop { get; }

        public ulong WhiteKnight { get; }
        public ulong BlackKnight { get; }

        public ulong WhitePawn { get; }
        public ulong BlackPawn { get; }

        public ulong WhitePieces => WhiteKing | WhiteQueen | WhiteRook | WhiteBishop | WhiteKnight | WhitePawn;
        public ulong BlackPieces => BlackKing | BlackQueen | BlackRook | BlackBishop | BlackKnight | BlackPawn;

        public ulong Occupied => WhitePieces | BlackPieces;
    }
}
