using Charlie.Moves;
using System.Collections.Generic;
using System.Numerics;

namespace Charlie.BoardRepresentation
{
    public class BoardState
    {
        private static string StartPositionFen { get; } =
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        private readonly List<long> previousStates;

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
            List<long> previousStates,
            Board bitBoard,
            PieceColour toMove,
            byte castleRules,
            ulong whiteEnPassant,
            ulong blackEnPassant)
        {
            Board = bitBoard;

            CastleRules = castleRules;

            WhiteEnPassant = whiteEnPassant;
            BlackEnPassant = blackEnPassant;

            ToMove = toMove;

            HashCode = CalculateLongHashCode();
            this.previousStates = new List<long>(previousStates) { HashCode };
        }

        private BoardState(
            List<long> previousStates,
            Board bitBoard,
            PieceColour toMove,
            byte castleRules,
            ulong whiteEnPassant,
            ulong blackEnPassant,
            Move move) : this(
                previousStates,
                new Board(bitBoard, move),
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

            Board = new Board(pieces);
            CastleRules = GetCastlingRulesFromFen(castlingRules);
            ToMove = toMove == "w" ? PieceColour.White : PieceColour.Black;
            WhiteEnPassant = GetEnPassantFromFen(enPassant[0], ToMove == PieceColour.White);
            BlackEnPassant = GetEnPassantFromFen(enPassant[0], ToMove == PieceColour.Black);
            HashCode = CalculateLongHashCode();
            previousStates = new List<long>() { HashCode };
        }

        private ulong GetEnPassantFromFen(char enPassantFile, bool whiteToMove)
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
            // Check if en passant will be possible next move
            ulong whiteEP = 0, blackEP = 0;

            if (move.IsDoublePush)
            {
                blackEP = move.ToCell << 8;
                whiteEP = move.ToCell >> 8;
            }

            // Check if castling rules have changed
            byte castleRules = CastleRules;

            if ((Board.WhiteRook & move.FromCell & Chessboard.SquareH1) != 0)
                castleRules &= unchecked((byte)~0b_00000001);

            if ((Board.WhiteRook & move.FromCell & Chessboard.SquareA1) != 0)
                castleRules &= unchecked((byte)~0b_00000010);

            if ((Board.WhiteKing & move.FromCell) != 0) castleRules &= unchecked((byte)~0b_00000011);

            if ((Board.BlackRook & move.FromCell & Chessboard.SquareH8) != 0)
                castleRules &= unchecked((byte)~0b_00000100);

            if ((Board.BlackRook & move.FromCell & Chessboard.SquareA8) != 0)
                castleRules &= unchecked((byte)~0b_00001000);

            if ((Board.BlackKing & move.FromCell) != 0) castleRules &= unchecked((byte)~0b_00001100);

            PieceColour nextToMove = ToMove == PieceColour.White ? PieceColour.Black : PieceColour.White;

            return new BoardState(
                previousStates,
                Board,
                nextToMove,
                castleRules,
                whiteEP,
                blackEP,
                move);
        }

        internal bool IsThreeMoveRepetition()
        {
            int count = 0;

            foreach (long state in previousStates)
            {
                if (state.Equals(HashCode))
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

                if ((Magics.BishopAttacks[cellIndex] & (Board.BlackBishop | Board.BlackQueen)) != 0) return true;
                if ((Magics.RookAttacks[cellIndex] & (Board.BlackRook | Board.BlackQueen)) != 0) return true;
            }
            else
            {
                if (IsUnderImmediateAttack(Board.BlackKing, Board.WhiteKing, attacker)) return true;
                if (IsUnderKnightAttack(Board.BlackKing, Board.WhiteKnight)) return true;

                int cellIndex = BitOperations.TrailingZeroCount(Board.BlackKing);

                if ((Magics.BishopAttacks[cellIndex] & (Board.WhiteBishop | Board.WhiteQueen)) != 0) return true;
                if ((Magics.RookAttacks[cellIndex] & (Board.WhiteRook | Board.WhiteQueen)) != 0) return true;
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
                if (up && right && ((cell >> 9) & Board.BlackPawn) != 0) return true;
                if (up && left && ((cell >> 7) & Board.BlackPawn) != 0) return true;
            }
            else
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

            // scan up
            int distance = 0;
            if ((Magics.RookAttacks[cellIndex] & ordinalSliders) != 0)
            {
                while (((cell >> distance) & ~Chessboard.Rank8) != 0)
                {
                    distance += 8;
                    ulong cellToCheck = cell >> distance;
                    if ((cellToCheck & ordinalSliders) != 0) return true;
                    if ((cellToCheck & occupiedBb) != 0) break;
                }

                // scan down
                distance = 0;
                while (((cell << distance) & ~Chessboard.Rank1) != 0)
                {
                    distance += 8;
                    ulong cellToCheck = cell << distance;
                    if ((cellToCheck & ordinalSliders) != 0) return true;
                    if ((cellToCheck & occupiedBb) != 0) break;
                }

                // scan right
                distance = 0;
                while (((cell >> distance) & ~Chessboard.HFile) != 0)
                {
                    distance++;
                    ulong cellToCheck = cell >> distance;
                    if ((cellToCheck & ordinalSliders) != 0) return true;
                    if ((cellToCheck & occupiedBb) != 0) break;
                }

                // scan left
                distance = 0;
                while (((cell << distance) & ~Chessboard.AFile) != 0)
                {
                    distance++;
                    ulong cellToCheck = cell << distance;
                    if ((cellToCheck & ordinalSliders) != 0) return true;
                    if ((cellToCheck & occupiedBb) != 0) break;
                }
            }

            if ((Magics.BishopAttacks[cellIndex] & diagonalSliders) != 0)
            {
                // scan up right
                distance = 0;
                while (((cell >> distance) & ~Chessboard.Rank8 & ~Chessboard.HFile) != 0)
                {
                    distance += 9;
                    ulong cellToCheck = cell >> distance;
                    if ((cellToCheck & diagonalSliders) != 0) return true;
                    if ((cellToCheck & occupiedBb) != 0) break;
                }

                // scan up left
                distance = 0;
                while (((cell >> distance) & ~Chessboard.Rank8 & ~Chessboard.AFile) != 0)
                {
                    distance += 7;
                    ulong cellToCheck = cell >> distance;
                    if ((cellToCheck & diagonalSliders) != 0) return true;
                    if ((cellToCheck & occupiedBb) != 0) break;
                }

                // scan down right
                distance = 0;
                while (((cell << distance) & ~Chessboard.Rank1 & ~Chessboard.HFile) != 0)
                {
                    distance += 7;
                    ulong cellToCheck = cell << distance;
                    if ((cellToCheck & diagonalSliders) != 0) return true;
                    if ((cellToCheck & occupiedBb) != 0) break;
                }

                // scan down left
                distance = 0;
                while (((cell << distance) & ~Chessboard.Rank1 & ~Chessboard.AFile) != 0)
                {
                    distance += 9;
                    ulong cellToCheck = cell << distance;
                    if ((cellToCheck & diagonalSliders) != 0) return true;
                    if ((cellToCheck & occupiedBb) != 0) break;
                }
            }

            return false;
        }

        private bool IsUnderKnightAttack(ulong cell, ulong theirKnight)
        {
            int cellIndex = BitOperations.TrailingZeroCount(cell);
            return (Magics.KnightAttacks[cellIndex] & theirKnight) != 0;
        }

        private long CalculateLongHashCode()
        {
            var hash = Board.GetLongHashCode() ^ (long)WhiteEnPassant ^ (long)BlackEnPassant;

            for (int i = 0; i < 64 / 4; i++)
                hash ^= CastleRules << i;

            if (ToMove == PieceColour.Black)
                hash ^= long.MaxValue;

            return hash;
        }
    }
}
