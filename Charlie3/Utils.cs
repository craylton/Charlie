﻿using System.Runtime.InteropServices;
using System.Security;

namespace Charlie3
{
    public static class Utils
    {
        public static ulong CountLeadingZeros(ulong input)
        {
            if (input == 0) return 0;

            ulong n = 1;

            if ((input >> 32) == 0) { n += 32; input <<= 32; }
            if ((input >> 48) == 0) { n += 16; input <<= 16; }
            if ((input >> 56) == 0) { n += 8; input <<= 8; }
            if ((input >> 60) == 0) { n += 4; input <<= 4; }
            if ((input >> 62) == 0) { n += 2; input <<= 2; }
            n -= input >> 63;

            return n;
        }

        public static int CountTrailingZeroes(ulong input)
        {
            return RtlFindLeastSignificantBit(input);
            //return 63 - CountLeadingZeros(input);
        }

        // This dll is apparently a Windows thing
        [DllImport("ntdll"), SuppressUnmanagedCodeSecurity]
        private static extern int RtlFindLeastSignificantBit(ulong ul);

        public static string[] CellNames { get; } = new string[] {
            "a1","b1","c1","d1","e1","f1","g1","h1",
            "a2","b2","c2","d2","e2","f2","g2","h2",
            "a3","b3","c3","d3","e3","f3","g3","h3",
            "a4","b4","c4","d4","e4","f4","g4","h4",
            "a5","b5","c5","d5","e5","f5","g5","h5",
            "a6","b6","c6","d6","e6","f6","g6","h6",
            "a7","b7","c7","d7","e7","f7","g7","h7",
            "a8","b8","c8","d8","e8","f8","g8","h8",
        };

        public static char[] PromotionSuffixes { get; } = new[] { '?', 'N', 'B', 'R', 'Q' };
    }
}
