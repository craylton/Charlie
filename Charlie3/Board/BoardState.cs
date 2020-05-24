using Charlie.Moves;
using System.Collections.Generic;
using System.Numerics;

namespace Charlie.Board
{
    public class BoardState
    {
        private readonly List<long> previousStates;

        public BitBoard BitBoard { get; }

        // 0001 = white short, 0010 = white long, 0100 = black short, 1000 = black long
        public byte CastleRules { get; }

        // For en-passants, the set bit is where the capturing pawn will end up. White = white can capture
        public ulong WhiteEnPassant { get; }
        public ulong BlackEnPassant { get; }

        public PieceColour ToMove { get; }

        public BoardState() : this(
            new List<long>(),
            BitBoard.GetDefault(),
            PieceColour.White,
            0b_00001111,
            0,
            0)
        {
        }

        private BoardState(
            List<long> previousStates,
            BitBoard bitBoard,
            PieceColour toMove,
            byte castleRules,
            ulong whiteEnPassant,
            ulong blackEnPassant)
        {
            BitBoard = bitBoard;

            CastleRules = castleRules;

            WhiteEnPassant = whiteEnPassant;
            BlackEnPassant = blackEnPassant;

            ToMove = toMove;

            this.previousStates = new List<long>(previousStates) { GetLongHashCode() };
        }

        private BoardState(
            List<long> previousStates,
            BitBoard bitBoard,
            PieceColour toMove,
            byte castleRules,
            ulong whiteEnPassant,
            ulong blackEnPassant,
            Move move) : this(
                previousStates,
                new BitBoard(bitBoard, move),
                toMove,
                castleRules,
                whiteEnPassant,
                blackEnPassant)
        {
        }

        public BoardState(string[] fenElements)
        {
            string pieces = fenElements[0];
            string toMove = fenElements[1];
            string castlingRules = fenElements[2];
            string enPassant = fenElements[3];
            string fiftyMoveRule = fenElements[4];
            string numberOfMoves = fenElements[5];

            BitBoard = new BitBoard(pieces);
            CastleRules = GetCastlingRulesFromFen(castlingRules);
            ToMove = toMove == "w" ? PieceColour.White : PieceColour.Black;

            if (enPassant == "-")
            {
                WhiteEnPassant = BlackEnPassant = 0;
            }
            else
            {
                if (ToMove == PieceColour.White)
                    WhiteEnPassant = GetEnPassantFromFen(enPassant[0], true);
                else
                    BlackEnPassant = GetEnPassantFromFen(enPassant[0], false);
            }

            previousStates = new List<long>();
        }

        private ulong GetEnPassantFromFen(char enPassantFile, bool whiteToMove)
        {
            int rank = 8 * (whiteToMove ? 3 : 6);
            int file = enPassantFile - 'a';
            return 1ul << (rank - file - 1);
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
            // Check if en passant will be possible next move
            ulong whiteEP = 0, blackEP = 0;

            if (move.IsDoublePush)
            {
                // if white pushed
                if (move.FromCell > move.ToCell)
                    blackEP = move.ToCell << 8;
                else
                    whiteEP = move.ToCell >> 8;
            }

            // Check if castling rules have changed
            byte castleRules = CastleRules;

            if ((BitBoard.WhiteRook & move.FromCell & Chessboard.SquareH1) != 0)
                castleRules &= unchecked((byte)~0b_00000001);

            if ((BitBoard.WhiteRook & move.FromCell & Chessboard.SquareA1) != 0)
                castleRules &= unchecked((byte)~0b_00000010);

            if ((BitBoard.WhiteKing & move.FromCell) != 0) castleRules &= unchecked((byte)~0b_00000011);

            if ((BitBoard.BlackRook & move.FromCell & Chessboard.SquareH8) != 0)
                castleRules &= unchecked((byte)~0b_00000100);

            if ((BitBoard.BlackRook & move.FromCell & Chessboard.SquareA8) != 0)
                castleRules &= unchecked((byte)~0b_00001000);

            if ((BitBoard.BlackKing & move.FromCell) != 0) castleRules &= unchecked((byte)~0b_00001100);

            PieceColour nextToMove = ToMove == PieceColour.White ? PieceColour.Black : PieceColour.White;

            return new BoardState(
                previousStates,
                BitBoard,
                nextToMove,
                castleRules,
                whiteEP,
                blackEP,
                move);
        }

        internal bool IsThreeMoveRepetition()
        {
            int count = 0;
            long thisHash = GetLongHashCode();

            foreach (long state in previousStates)
            {
                if (state.Equals(thisHash))
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
                return IsUnderAttack(BitBoard.WhiteKing, PieceColour.Black);
            else
                return IsUnderAttack(BitBoard.BlackKing, PieceColour.White);
        }

        internal bool IsInPseudoCheck(PieceColour attacker)
        {
            if (attacker == PieceColour.Black)
            {
                if (IsUnderImmediateAttack(BitBoard.WhiteKing, BitBoard.BlackKing, attacker)) return true;
                if (IsUnderKnightAttack(BitBoard.WhiteKing, BitBoard.BlackKnight)) return true;

                int cellIndex = BitOperations.TrailingZeroCount(BitBoard.WhiteKing);

                if ((Magics.BishopAttacks[cellIndex] & (BitBoard.BlackBishop | BitBoard.BlackQueen)) != 0) return true;
                if ((Magics.RookAttacks[cellIndex] & (BitBoard.BlackRook | BitBoard.BlackQueen)) != 0) return true;
            }
            else
            {
                if (IsUnderImmediateAttack(BitBoard.BlackKing, BitBoard.WhiteKing, attacker)) return true;
                if (IsUnderKnightAttack(BitBoard.BlackKing, BitBoard.WhiteKnight)) return true;

                int cellIndex = BitOperations.TrailingZeroCount(BitBoard.BlackKing);

                if ((Magics.BishopAttacks[cellIndex] & (BitBoard.WhiteBishop | BitBoard.WhiteQueen)) != 0) return true;
                if ((Magics.RookAttacks[cellIndex] & (BitBoard.WhiteRook | BitBoard.WhiteQueen)) != 0) return true;
            }

            return false;
        }

        internal bool IsUnderAttack(ulong cell, PieceColour attacker)
        {
            if (attacker == PieceColour.Black)
            {
                if (IsUnderImmediateAttack(cell, BitBoard.BlackKing, attacker)) return true;
                if (IsUnderRayAttack(cell, BitBoard.BlackQueen, BitBoard.BlackRook, BitBoard.BlackBishop)) return true;
                if (IsUnderKnightAttack(cell, BitBoard.BlackKnight)) return true;
            }
            else
            {
                if (IsUnderImmediateAttack(cell, BitBoard.WhiteKing, attacker)) return true;
                if (IsUnderRayAttack(cell, BitBoard.WhiteQueen, BitBoard.WhiteRook, BitBoard.WhiteBishop)) return true;
                if (IsUnderKnightAttack(cell, BitBoard.WhiteKnight)) return true;
            }

            return false;
        }

        private bool IsUnderImmediateAttack(ulong cell, ulong theirKing, PieceColour attacker)
        {
            bool up = (cell & ~Chessboard.Rank8) != 0,
            down = (cell & ~Chessboard.Rank1) != 0,
            right = (cell & ~Chessboard.HFile) != 0,
            left = (cell & ~Chessboard.AFile) != 0;

            if (up && ((cell >> 8) & theirKing) != 0) return true;
            if (down && ((cell << 8) & theirKing) != 0) return true;
            if (right && ((cell >> 1) & theirKing) != 0) return true;
            if (left && ((cell << 1) & theirKing) != 0) return true;
            if (up && right && ((cell >> 9) & theirKing) != 0) return true;
            if (up && left && ((cell >> 7) & theirKing) != 0) return true;
            if (down && right && ((cell << 7) & theirKing) != 0) return true;
            if (down && left && ((cell << 9) & theirKing) != 0) return true;

            if (attacker == PieceColour.Black)
            {
                if (up && right && ((cell >> 9) & BitBoard.BlackPawn) != 0) return true;
                if (up && left && ((cell >> 7) & BitBoard.BlackPawn) != 0) return true;
            }
            else
            {
                if (down && right && ((cell << 7) & BitBoard.WhitePawn) != 0) return true;
                if (down && left && ((cell << 9) & BitBoard.WhitePawn) != 0) return true;
            }

            return false;
        }

        private bool IsUnderRayAttack(ulong cell, ulong theirQueen, ulong theirRook, ulong theirBishop)
        {
            ulong occupiedBb = BitBoard.Occupied;

            // scan up
            int distance = 0;
            while (((cell >> distance) & ~Chessboard.Rank8) != 0)
            {
                distance += 8;
                if (((cell >> distance) & (theirRook | theirQueen)) != 0) return true;
                if (((cell >> distance) & occupiedBb) != 0) break;
            }

            // scan down
            distance = 0;
            while (((cell << distance) & ~Chessboard.Rank1) != 0)
            {
                distance += 8;
                if (((cell << distance) & (theirRook | theirQueen)) != 0) return true;
                if (((cell << distance) & occupiedBb) != 0) break;
            }

            // scan right
            distance = 0;
            while (((cell >> distance) & ~Chessboard.HFile) != 0)
            {
                distance++;
                if (((cell >> distance) & (theirRook | theirQueen)) != 0) return true;
                if (((cell >> distance) & occupiedBb) != 0) break;
            }

            // scan left
            distance = 0;
            while (((cell << distance) & ~Chessboard.AFile) != 0)
            {
                distance++;
                if (((cell << distance) & (theirRook | theirQueen)) != 0) return true;
                if (((cell << distance) & occupiedBb) != 0) break;
            }

            // scan up right
            distance = 0;
            while (((cell >> distance) & ~Chessboard.Rank8 & ~Chessboard.HFile) != 0)
            {
                distance += 9;
                if (((cell >> distance) & (theirBishop | theirQueen)) != 0) return true;
                if (((cell >> distance) & occupiedBb) != 0) break;
            }

            // scan up left
            distance = 0;
            while (((cell >> distance) & ~Chessboard.Rank8 & ~Chessboard.AFile) != 0)
            {
                distance += 7;
                if (((cell >> distance) & (theirBishop | theirQueen)) != 0) return true;
                if (((cell >> distance) & occupiedBb) != 0) break;
            }

            // scan down right
            distance = 0;
            while (((cell << distance) & ~Chessboard.Rank1 & ~Chessboard.HFile) != 0)
            {
                distance += 7;
                if (((cell << distance) & (theirBishop | theirQueen)) != 0) return true;
                if (((cell << distance) & occupiedBb) != 0) break;
            }

            // scan down left
            distance = 0;
            while (((cell << distance) & ~Chessboard.Rank1 & ~Chessboard.AFile) != 0)
            {
                distance += 9;
                if (((cell << distance) & (theirBishop | theirQueen)) != 0) return true;
                if (((cell << distance) & occupiedBb) != 0) break;
            }

            return false;
        }

        private bool IsUnderKnightAttack(ulong cell, ulong theirKnight)
        {
            int cellIndex = BitOperations.TrailingZeroCount(cell);
            return (Magics.KnightAttacks[cellIndex] & theirKnight) != 0;
        }

        public long GetLongHashCode()
        {
            var hash = BitBoard.GetLongHashCode() ^ (long)WhiteEnPassant ^ (long)BlackEnPassant;

            for (int i = 0; i < 64 / 4; i++)
                hash ^= CastleRules << i;

            if (ToMove == PieceColour.Black)
                hash ^= long.MaxValue;

            return hash;
        }
    }
}
