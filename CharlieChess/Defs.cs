/*
 *	DEFS.H
 *	Tom Kerrigan's Simple Chess Program (TSCP)
 *
 *	Copyright 1997 Tom Kerrigan
 */

/* with fen and null move capabilities - N.Blais 3/5/05 */
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TSCP_Sharp_unsafe
{
    public partial class TSCP
    {
        struct move_bytes
        {
            public sbyte from;
            public sbyte to;
            public sbyte promote;
            public sbyte bits;
        }

        [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
         struct move
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public move_bytes b;
            [System.Runtime.InteropServices.FieldOffset(0)]
            public long u;
        }

         class gen_t
        {
            public move m;
            public long score;
        }

         struct hist_t
        {
            public move m;
            public long capture;
            public long castle;
            public long ep;
            public long fifty;
            public long hash;
        }

        const long GEN_STACK = 1120;
        const long MAX_PLY = 32;
        const long HIST_STACK = 400;

        const long LIGHT = 0;
        const long DARK = 1;

        const long PAWN = 0;
        const long KNIGHT = 1;
        const long BISHOP = 2;
        const long ROOK = 3;
        const long QUEEN = 4;
        const long KING = 5;

        const long EMPTY = 6;

        const long A1 = 56;
        const long B1 = 57;
        const long C1 = 58;
        const long D1 = 59;
        const long E1 = 60;
        const long F1 = 61;
        const long G1 = 62;
        const long H1 = 63;
        const long A8 = 0;
        const long B8 = 1;
        const long C8 = 2;
        const long D8 = 3;
        const long E8 = 4;
        const long F8 = 5;
        const long G8 = 6;
        const long H8 = 7;

    }
}
