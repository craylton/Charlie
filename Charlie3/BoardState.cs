using System;
using System.Collections.Generic;

namespace Charlie3
{
    public class BoardState : ICloneable
    {
        private IEnumerable<BoardState> previousStates;

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
            IEnumerable<BoardState> previousStates,
            BitBoard bitBoard, PieceColour toMove,
            int whiteCastle, int blackCastle,
            ulong whiteEnPassant, ulong blackEnPassant)
        {
            this.previousStates = previousStates;

            BitBoard = bitBoard;

            WhiteCastle = whiteCastle;
            BlackCastle = blackCastle;

            WhiteEnPassant = whiteEnPassant;
            BlackEnPassant = blackEnPassant;

            ToMove = toMove;
        }

        private BoardState(
            IEnumerable<BoardState> previousStates,
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

        internal bool IsInCheck(PieceColour toMove)
        {
            if (toMove == PieceColour.White)
            {
                if (IsInImmediateCheck(BitBoard.WhiteKing, BitBoard.BlackKing, toMove)) return true;
                if (IsInRayCheck(BitBoard.WhiteKing, BitBoard.BlackQueen, BitBoard.BlackRook, BitBoard.BlackBishop)) return true;
                if (IsInKnightCheck(BitBoard.WhiteKing, BitBoard.BlackKnight)) return true;
            }
            else
            {
                if (IsInImmediateCheck(BitBoard.BlackKing, BitBoard.WhiteKing, toMove)) return true;
                if (IsInRayCheck(BitBoard.BlackKing, BitBoard.WhiteQueen, BitBoard.WhiteRook, BitBoard.WhiteBishop)) return true;
                if (IsInKnightCheck(BitBoard.BlackKing, BitBoard.WhiteKnight)) return true;
            }

            return false;
        }

        private bool IsInImmediateCheck(ulong king, ulong theirKing, PieceColour toMove)
        {
            bool up = (king & ~0x00_00_00_00_00_00_00_FFul) != 0,
            down = (king & ~0xFF_00_00_00_00_00_00_00ul) != 0,
            right = (king & ~0x01_01_01_01_01_01_01_01ul) != 0,
            left = (king & ~0x80_80_80_80_80_80_80_80ul) != 0;

            if (up && ((king >> 8) & theirKing) != 0) return true;
            if (down && ((king << 8) & theirKing) != 0) return true;
            if (right && ((king >> 1) & theirKing) != 0) return true;
            if (left && ((king << 1) & theirKing) != 0) return true;
            if (up && right && ((king >> 9) & theirKing) != 0) return true;
            if (up && left && ((king >> 7) & theirKing) != 0) return true;
            if (down && right && ((king << 7) & theirKing) != 0) return true;
            if (down && left && ((king << 9) & theirKing) != 0) return true;

            if (toMove == PieceColour.White)
            {
                if (up && right && ((king >> 9) & BitBoard.BlackPawn) != 0) return true;
                if (up && left && ((king >> 7) & BitBoard.BlackPawn) != 0) return true;
            }
            else
            {
                if (down && right && ((king << 7) & BitBoard.WhitePawn) != 0) return true;
                if (down && left && ((king << 9) & BitBoard.WhitePawn) != 0) return true;
            }

            return false;
        }

        private bool IsInRayCheck(ulong king, ulong theirQueen, ulong theirRook, ulong theirBishop)
        {
            // scan up
            int distance = 0;
            while (((king >> distance) & ~0x00_00_00_00_00_00_00_FFul) != 0)
            {
                distance += 8;
                if (((king >> distance) & (theirRook | theirQueen)) != 0) return true;
                if (((king >> distance) & BitBoard.Occupied) != 0) break;
            }

            // scan down
            distance = 0;
            while (((king << distance) & ~0xFF_00_00_00_00_00_00_00ul) != 0)
            {
                distance += 8;
                if (((king << distance) & (theirRook | theirQueen)) != 0) return true;
                if (((king << distance) & BitBoard.Occupied) != 0) break;
            }

            // scan right
            distance = 0;
            while (((king >> distance) & ~0x01_01_01_01_01_01_01_01ul) != 0)
            {
                distance++;
                if (((king >> distance) & (theirRook | theirQueen)) != 0) return true;
                if (((king >> distance) & BitBoard.Occupied) != 0) break;
            }

            // scan left
            distance = 0;
            while (((king << distance) & ~0x80_80_80_80_80_80_80_80ul) != 0)
            {
                distance++;
                if (((king << distance) & (theirRook | theirQueen)) != 0) return true;
                if (((king << distance) & BitBoard.Occupied) != 0) break;
            }

            // scan up right
            distance = 0;
            while (((king >> distance) & ~0x00_00_00_00_00_00_00_FFul & ~0x01_01_01_01_01_01_01_01ul) != 0)
            {
                distance += 9;
                if (((king >> distance) & (theirBishop | theirQueen)) != 0) return true;
                if (((king >> distance) & BitBoard.Occupied) != 0) break;
            }

            // scan up left
            distance = 0;
            while (((king >> distance) & ~0x00_00_00_00_00_00_00_FFul & ~0x80_80_80_80_80_80_80_80ul) != 0)
            {
                distance += 7;
                if (((king >> distance) & (theirBishop | theirQueen)) != 0) return true;
                if (((king >> distance) & BitBoard.Occupied) != 0) break;
            }

            // scan down right
            distance = 0;
            while (((king << distance) & ~0xFF_00_00_00_00_00_00_00ul & ~0x01_01_01_01_01_01_01_01ul) != 0)
            {
                distance += 7;
                if (((king << distance) & (theirBishop | theirQueen)) != 0) return true;
                if (((king << distance) & BitBoard.Occupied) != 0) break;
            }

            // scan down left
            distance = 0;
            while (((king << distance) & ~0xFF_00_00_00_00_00_00_00ul & ~0x80_80_80_80_80_80_80_80ul) != 0)
            {
                distance += 9;
                if (((king << distance) & (theirBishop | theirQueen)) != 0) return true;
                if (((king << distance) & BitBoard.Occupied) != 0) break;
            }

            return false;
        }

        private bool IsInKnightCheck(ulong king, ulong theirKnight)
        {
            bool up = (king & ~0x00_00_00_00_00_00_00_FFul) != 0,
            up2 = (king & ~0x00_00_00_00_00_00_FF_FFul) != 0,
            down = (king & ~0xFF_00_00_00_00_00_00_00ul) != 0,
            down2 = (king & ~0xFF_FF_00_00_00_00_00_00ul) != 0,
            right = (king & ~0x01_01_01_01_01_01_01_01ul) != 0,
            right2 = (king & ~0x03_03_03_03_03_03_03_03ul) != 0,
            left = (king & ~0x80_80_80_80_80_80_80_80ul) != 0,
            left2 = (king & ~0xC0_C0_C0_C0_C0_C0_C0_C0ul) != 0;

            if (up2 && right && ((king >> 17) & theirKnight) != 0) return true;
            if (up && right2 && ((king >> 10) & theirKnight) != 0) return true;
            if (down && right2 && ((king << 6) & theirKnight) != 0) return true;
            if (down2 && right && ((king << 15) & theirKnight) != 0) return true;
            if (down2 && left && ((king << 17) & theirKnight) != 0) return true;
            if (down && left2 && ((king << 10) & theirKnight) != 0) return true;
            if (up && left2 && ((king >> 6) & theirKnight) != 0) return true;
            if (up2 && left && ((king >> 15) & theirKnight) != 0) return true;

            return false;
        }

        public object Clone() => new BoardState(
            previousStates, BitBoard, ToMove,
            WhiteCastle, BlackCastle,
            WhiteEnPassant, BlackEnPassant);
    }
}
