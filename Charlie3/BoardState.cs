using System;
using System.Collections.Generic;

namespace Charlie3
{
    public class BoardState : ICloneable
    {
        private readonly List<BoardState> previousStates;

        public BitBoard BitBoard { get; }

        // For castle, 01 means short castle, 10 means long
        public int WhiteCastle { get; }
        public int BlackCastle { get; }

        // For en-passants, the set bit is where the capturing pawn will end up. White = white can capture
        public ulong WhiteEnPassant { get; }
        public ulong BlackEnPassant { get; }

        public PieceColour ToMove { get; }

        public BoardState() :
            this(new List<BoardState>(),
                BitBoard.GetDefault(),
                PieceColour.White,
                0b_00000011, 0b_00000011, 0, 0)
        {
        }

        private BoardState(
            List<BoardState> previousStates,
            BitBoard bitBoard, PieceColour toMove,
            int whiteCastle, int blackCastle,
            ulong whiteEnPassant, ulong blackEnPassant)
        {
            this.previousStates = new List<BoardState>(previousStates) { this };

            BitBoard = bitBoard;

            WhiteCastle = whiteCastle;
            BlackCastle = blackCastle;

            WhiteEnPassant = whiteEnPassant;
            BlackEnPassant = blackEnPassant;

            ToMove = toMove;
        }

        private BoardState(
            List<BoardState> previousStates,
            BitBoard bitBoard, PieceColour toMove,
            int whiteCastle, int blackCastle,
            ulong whiteEnPassant, ulong blackEnPassant,
            Move move) :
            this(previousStates, new BitBoard(bitBoard, move), toMove,
                whiteCastle, blackCastle,
                whiteEnPassant, blackEnPassant)
        {
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
            int whiteCastle = WhiteCastle, blackCastle = BlackCastle;
            if ((BitBoard.WhiteRook & move.FromCell & 0x01_00_00_00_00_00_00_00) != 0)
                whiteCastle &= ~0b_00000001;

            if ((BitBoard.WhiteRook & move.FromCell & 0x80_00_00_00_00_00_00_00) != 0)
                whiteCastle &= ~0b_00000010;

            if ((BitBoard.WhiteKing & move.FromCell) != 0) whiteCastle = 0;

            if ((BitBoard.BlackRook & move.FromCell & 0x00_00_00_00_00_00_00_01) != 0)
                blackCastle &= ~0b_00000001;

            if ((BitBoard.BlackRook & move.FromCell & 0x00_00_00_00_00_00_00_80) != 0)
                blackCastle &= ~0b_00000010;

            if ((BitBoard.BlackKing & move.FromCell) != 0) blackCastle = 0;

            PieceColour nextToMove = ToMove == PieceColour.White ? PieceColour.Black : PieceColour.White;

            return new BoardState(
                previousStates, BitBoard, nextToMove,
                whiteCastle, blackCastle,
                whiteEP, blackEP, move);
        }

        internal bool IsThreeMoveRepetition()
        {
            int count = 1;
            foreach (var state in previousStates)
            {
                if (state == this) continue;
                if (state.BitBoard.Equals(BitBoard) && ++count == 3) return true;
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

        internal bool IsUnderAttack(ulong cell, PieceColour attacker)
        {
            if (attacker == PieceColour.Black)
            {
                if (IsUnderImmediateAttack(cell, BitBoard.BlackKing, attacker)) return true;
                if (IsUnderRayAttack(cell, BitBoard.BlackQueen, BitBoard.BlackRook, BitBoard.BlackBishop)) return true;
                if (IsInKnightCheck(cell, BitBoard.BlackKnight)) return true;
            }
            else
            {
                if (IsUnderImmediateAttack(cell, BitBoard.WhiteKing, attacker)) return true;
                if (IsUnderRayAttack(cell, BitBoard.WhiteQueen, BitBoard.WhiteRook, BitBoard.WhiteBishop)) return true;
                if (IsInKnightCheck(cell, BitBoard.WhiteKnight)) return true;
            }

            return false;
        }

        private bool IsUnderImmediateAttack(ulong cell, ulong theirKing, PieceColour attacker)
        {
            bool up = (cell & ~0x00_00_00_00_00_00_00_FFul) != 0,
            down = (cell & ~0xFF_00_00_00_00_00_00_00ul) != 0,
            right = (cell & ~0x01_01_01_01_01_01_01_01ul) != 0,
            left = (cell & ~0x80_80_80_80_80_80_80_80ul) != 0;

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
            var occupiedBb = BitBoard.Occupied;
            // scan up
            int distance = 0;
            while (((cell >> distance) & ~0x00_00_00_00_00_00_00_FFul) != 0)
            {
                distance += 8;
                if (((cell >> distance) & (theirRook | theirQueen)) != 0) return true;
                if (((cell >> distance) & occupiedBb) != 0) break;
            }

            // scan down
            distance = 0;
            while (((cell << distance) & ~0xFF_00_00_00_00_00_00_00ul) != 0)
            {
                distance += 8;
                if (((cell << distance) & (theirRook | theirQueen)) != 0) return true;
                if (((cell << distance) & occupiedBb) != 0) break;
            }

            // scan right
            distance = 0;
            while (((cell >> distance) & ~0x01_01_01_01_01_01_01_01ul) != 0)
            {
                distance++;
                if (((cell >> distance) & (theirRook | theirQueen)) != 0) return true;
                if (((cell >> distance) & occupiedBb) != 0) break;
            }

            // scan left
            distance = 0;
            while (((cell << distance) & ~0x80_80_80_80_80_80_80_80ul) != 0)
            {
                distance++;
                if (((cell << distance) & (theirRook | theirQueen)) != 0) return true;
                if (((cell << distance) & occupiedBb) != 0) break;
            }

            // scan up right
            distance = 0;
            while (((cell >> distance) & ~0x00_00_00_00_00_00_00_FFul & ~0x01_01_01_01_01_01_01_01ul) != 0)
            {
                distance += 9;
                if (((cell >> distance) & (theirBishop | theirQueen)) != 0) return true;
                if (((cell >> distance) & occupiedBb) != 0) break;
            }

            // scan up left
            distance = 0;
            while (((cell >> distance) & ~0x00_00_00_00_00_00_00_FFul & ~0x80_80_80_80_80_80_80_80ul) != 0)
            {
                distance += 7;
                if (((cell >> distance) & (theirBishop | theirQueen)) != 0) return true;
                if (((cell >> distance) & occupiedBb) != 0) break;
            }

            // scan down right
            distance = 0;
            while (((cell << distance) & ~0xFF_00_00_00_00_00_00_00ul & ~0x01_01_01_01_01_01_01_01ul) != 0)
            {
                distance += 7;
                if (((cell << distance) & (theirBishop | theirQueen)) != 0) return true;
                if (((cell << distance) & occupiedBb) != 0) break;
            }

            // scan down left
            distance = 0;
            while (((cell << distance) & ~0xFF_00_00_00_00_00_00_00ul & ~0x80_80_80_80_80_80_80_80ul) != 0)
            {
                distance += 9;
                if (((cell << distance) & (theirBishop | theirQueen)) != 0) return true;
                if (((cell << distance) & occupiedBb) != 0) break;
            }

            return false;
        }

        private bool IsInKnightCheck(ulong cell, ulong theirKnight)
        {
            bool up = (cell & ~0x00_00_00_00_00_00_00_FFul) != 0,
            up2 = (cell & ~0x00_00_00_00_00_00_FF_FFul) != 0,
            down = (cell & ~0xFF_00_00_00_00_00_00_00ul) != 0,
            down2 = (cell & ~0xFF_FF_00_00_00_00_00_00ul) != 0,
            right = (cell & ~0x01_01_01_01_01_01_01_01ul) != 0,
            right2 = (cell & ~0x03_03_03_03_03_03_03_03ul) != 0,
            left = (cell & ~0x80_80_80_80_80_80_80_80ul) != 0,
            left2 = (cell & ~0xC0_C0_C0_C0_C0_C0_C0_C0ul) != 0;

            if (up2 && right && ((cell >> 17) & theirKnight) != 0) return true;
            if (up && right2 && ((cell >> 10) & theirKnight) != 0) return true;
            if (down && right2 && ((cell << 6) & theirKnight) != 0) return true;
            if (down2 && right && ((cell << 15) & theirKnight) != 0) return true;
            if (down2 && left && ((cell << 17) & theirKnight) != 0) return true;
            if (down && left2 && ((cell << 10) & theirKnight) != 0) return true;
            if (up && left2 && ((cell >> 6) & theirKnight) != 0) return true;
            if (up2 && left && ((cell >> 15) & theirKnight) != 0) return true;

            return false;
        }

        public object Clone() => new BoardState(
            previousStates, BitBoard, ToMove,
            WhiteCastle, BlackCastle,
            WhiteEnPassant, BlackEnPassant);
    }
}
