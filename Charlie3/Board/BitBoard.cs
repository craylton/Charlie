using Charlie.Hash;
using Charlie.Moves;
using System.Numerics;

namespace Charlie.Board
{
    public readonly struct BitBoard
    {
        private const ulong DefaultWhiteKing = Chessboard.SquareE1;
        private const ulong DefaultBlackKing = Chessboard.SquareE8;
        private const ulong DefaultWhiteQueen = Chessboard.SquareD1;
        private const ulong DefaultBlackQueen = Chessboard.SquareD8;
        private const ulong DefaultWhiteRook = Chessboard.SquareA1 | Chessboard.SquareH1;
        private const ulong DefaultBlackRook = Chessboard.SquareA8 | Chessboard.SquareH8;
        private const ulong DefaultWhiteBishop = Chessboard.SquareC1 | Chessboard.SquareF1;
        private const ulong DefaultBlackBishop = Chessboard.SquareC8 | Chessboard.SquareF8;
        private const ulong DefaultWhiteKnight = Chessboard.SquareB1 | Chessboard.SquareG1;
        private const ulong DefaultBlackKnight = Chessboard.SquareB8 | Chessboard.SquareG8;
        private const ulong DefaultWhitePawn = Chessboard.Rank2;
        private const ulong DefaultBlackPawn = Chessboard.Rank7;

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
                if (move.ToCell == Chessboard.SquareC1)
                {
                    WhiteKing = move.ToCell;
                    WhiteRook &= ~Chessboard.SquareA1;
                    WhiteRook |= Chessboard.SquareD1;
                }
                // White short castle
                else if (move.ToCell == Chessboard.SquareG1)
                {
                    WhiteKing = move.ToCell;
                    WhiteRook &= ~Chessboard.SquareH1;
                    WhiteRook |= Chessboard.SquareF1;
                }
                // Black long castle
                else if (move.ToCell == Chessboard.SquareC8)
                {
                    BlackKing = move.ToCell;
                    BlackRook &= ~Chessboard.SquareA8;
                    BlackRook |= Chessboard.SquareD8;
                }
                // Black short castle
                else if (move.ToCell == Chessboard.SquareG8)
                {
                    BlackKing = move.ToCell;
                    BlackRook &= ~Chessboard.SquareH8;
                    BlackRook |= Chessboard.SquareF8;
                }

                return;
            }

            ulong squaresToKeep = ~move.FromCell & ~move.ToCell;

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
                bool whitePromoted = (move.ToCell & Chessboard.Rank8) != 0;

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

        public BitBoard(string fenPieces) : this()
        {
            var cell = 0;

            for (int i = 0; i < fenPieces.Length; i++)
            {
                if (fenPieces[i] == '/')
                    continue;

                if (int.TryParse(fenPieces[i].ToString(), out int spaces))
                {
                    cell += spaces;
                    continue;
                }

                ulong currentCell = 1ul << (cell + 7 - 2 * (cell % 8));

                switch (fenPieces[i])
                {
                    case 'p':
                        BlackPawn |= currentCell;
                        break;
                    case 'P':
                        WhitePawn |= currentCell;
                        break;
                    case 'n':
                        BlackKnight |= currentCell;
                        break;
                    case 'N':
                        WhiteKnight |= currentCell;
                        break;
                    case 'b':
                        BlackBishop |= currentCell;
                        break;
                    case 'B':
                        WhiteBishop |= currentCell;
                        break;
                    case 'r':
                        BlackRook |= currentCell;
                        break;
                    case 'R':
                        WhiteRook |= currentCell;
                        break;
                    case 'q':
                        BlackQueen |= currentCell;
                        break;
                    case 'Q':
                        WhiteQueen |= currentCell;
                        break;
                    case 'k':
                        BlackKing |= currentCell;
                        break;
                    case 'K':
                        WhiteKing |= currentCell;
                        break;
                }

                cell++;
            }
        }

        public static BitBoard GetDefault() => new BitBoard(
            new[] { DefaultWhiteKing, DefaultBlackKing },
            new[] { DefaultWhiteQueen, DefaultBlackQueen },
            new[] { DefaultWhiteRook, DefaultBlackRook },
            new[] { DefaultWhiteBishop, DefaultBlackBishop },
            new[] { DefaultWhiteKnight, DefaultBlackKnight },
            new[] { DefaultWhitePawn, DefaultBlackPawn });

        public long GetLongHashCode()
        {
            var hash = 0L;

            var piece = WhiteKing;
            while (piece != 0)
            {
                var cellNumber = BitOperations.TrailingZeroCount(piece);
                hash ^= Zobrist.Keys[(int)PieceType.WhiteKing, cellNumber];
                piece ^= 1ul << cellNumber;
            }

            piece = BlackKing;
            while (piece != 0)
            {
                var cellNumber = BitOperations.TrailingZeroCount(piece);
                hash ^= Zobrist.Keys[(int)PieceType.BlackKing, cellNumber];
                piece ^= 1ul << cellNumber;
            }

            piece = WhiteQueen;
            while (piece != 0)
            {
                var cellNumber = BitOperations.TrailingZeroCount(piece);
                hash ^= Zobrist.Keys[(int)PieceType.WhiteQueen, cellNumber];
                piece ^= 1ul << cellNumber;
            }

            piece = BlackQueen;
            while (piece != 0)
            {
                var cellNumber = BitOperations.TrailingZeroCount(piece);
                hash ^= Zobrist.Keys[(int)PieceType.BlackQueen, cellNumber];
                piece ^= 1ul << cellNumber;
            }

            piece = WhiteRook;
            while (piece != 0)
            {
                var cellNumber = BitOperations.TrailingZeroCount(piece);
                hash ^= Zobrist.Keys[(int)PieceType.WhiteRook, cellNumber];
                piece ^= 1ul << cellNumber;
            }

            piece = BlackRook;
            while (piece != 0)
            {
                var cellNumber = BitOperations.TrailingZeroCount(piece);
                hash ^= Zobrist.Keys[(int)PieceType.BlackRook, cellNumber];
                piece ^= 1ul << cellNumber;
            }

            piece = WhiteBishop;
            while (piece != 0)
            {
                var cellNumber = BitOperations.TrailingZeroCount(piece);
                hash ^= Zobrist.Keys[(int)PieceType.WhiteBishop, cellNumber];
                piece ^= 1ul << cellNumber;
            }

            piece = BlackBishop;
            while (piece != 0)
            {
                var cellNumber = BitOperations.TrailingZeroCount(piece);
                hash ^= Zobrist.Keys[(int)PieceType.BlackBishop, cellNumber];
                piece ^= 1ul << cellNumber;
            }

            piece = WhiteKnight;
            while (piece != 0)
            {
                var cellNumber = BitOperations.TrailingZeroCount(piece);
                hash ^= Zobrist.Keys[(int)PieceType.WhiteKnight, cellNumber];
                piece ^= 1ul << cellNumber;
            }

            piece = BlackKnight;
            while (piece != 0)
            {
                var cellNumber = BitOperations.TrailingZeroCount(piece);
                hash ^= Zobrist.Keys[(int)PieceType.BlackKnight, cellNumber];
                piece ^= 1ul << cellNumber;
            }

            piece = WhitePawn;
            while (piece != 0)
            {
                var cellNumber = BitOperations.TrailingZeroCount(piece);
                hash ^= Zobrist.Keys[(int)PieceType.WhitePawn, cellNumber];
                piece ^= 1ul << cellNumber;
            }

            piece = BlackPawn;
            while (piece != 0)
            {
                var cellNumber = BitOperations.TrailingZeroCount(piece);
                hash ^= Zobrist.Keys[(int)PieceType.BlackPawn, cellNumber];
                piece ^= 1ul << cellNumber;
            }

            return hash;
        }
    }
}
