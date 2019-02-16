using System;
using System.Collections.Generic;

namespace Charlie3
{
    public class BoardState : ICloneable
    {
        private IEnumerable<BoardState> previousStates;

        public BitBoard BitBoard { get; }

        // For castle, 01 means short castle, 10 means long
        public byte WhiteCastle { get; }
        public byte BlackCastle { get; }

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
            byte whiteCastle, byte blackCastle,
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
            byte whiteCastle, byte blackCastle,
            ulong whiteEnPassant, ulong blackEnPassant,
            Move move) :
            this(previousStates, new BitBoard(bitBoard, move), toMove,
                whiteCastle, blackCastle,
                whiteEnPassant, blackEnPassant)
        {
        }

        public BoardState MakeMove(Move move)
        {
            ulong whiteEP = 0, blackEP = 0;
            if (move.IsDoublePush)
            {
                // if black pushed
                if (move.FromCell > move.ToCell)
                    whiteEP = move.ToCell >> 8;
                else
                    blackEP = move.ToCell << 8;
            }

            return new BoardState(
                previousStates, BitBoard, ToMove,
                WhiteCastle, BlackCastle,
                whiteEP, blackEP, move);
        }

        public object Clone() => new BoardState(
            previousStates, BitBoard, ToMove,
            WhiteCastle, BlackCastle,
            WhiteEnPassant, BlackEnPassant);

    }
}
