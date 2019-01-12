/*
 *	DEFS.H
 *	Tom Kerrigan's Simple Chess Program (TSCP)
 *
 *	Copyright 1997 Tom Kerrigan
 */

/* with fen and null move capabilities - N.Blais 3/5/05 */

using System.Runtime.InteropServices;

namespace CharlieChess
{
    public partial class Tscp
    {
        private struct MoveBytes
        {
            public sbyte from;
            public sbyte to;
            public sbyte promote;
            public sbyte bits;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Move
        {
            [FieldOffset(0)]
            public MoveBytes b;

            [FieldOffset(0)]
            public long u;
        }

        private class GenT
        {
            public Move m;
            public long score;
        }

        private struct HistT
        {
            public Move m;
            public long capture;
            public long castle;
            public long ep;
            public long fifty;
            public long hash;
        }

        private const long GEN_STACK = 1120;
        private const long MAX_PLY = 32;
        private const long HIST_STACK = 400;

        private const long LIGHT = 0;
        private const long DARK = 1;

        private const long PAWN = 0;
        private const long KNIGHT = 1;
        private const long BISHOP = 2;
        private const long ROOK = 3;
        private const long QUEEN = 4;
        private const long KING = 5;

        private const long EMPTY = 6;

        private const long A1 = 56;
        private const long B1 = 57;
        private const long C1 = 58;
        private const long D1 = 59;
        private const long E1 = 60;
        private const long F1 = 61;
        private const long G1 = 62;
        private const long H1 = 63;
        private const long A8 = 0;
        private const long B8 = 1;
        private const long C8 = 2;
        private const long D8 = 3;
        private const long E8 = 4;
        private const long F8 = 5;
        private const long G8 = 6;
        private const long H8 = 7;
    }
}
