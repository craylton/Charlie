using System;

namespace Charlie.BoardRepresentation;

public static class Magics
{
    // 64 squares * 4 directions, in order: up right left down
    public static ulong[,][] TargetedRookAttacks { get; private set; }

    public static ulong[] AllRookAttacks { get; private set; }

    // 64 squares * 4 directions, in order: up-right up-left down-right down-left
    public static ulong[,][] TargetedBishopAttacks { get; private set; }

    public static ulong[] AllBishopAttacks { get; private set; }

    public static ulong[] Neighbours { get; private set; }

    public static void Initialise()
    {
        GenerateRookAttacks();
        GenerateBishopAttacks();
        GenerateNeighbours();
    }

    private static void GenerateRookAttacks()
    {
        TargetedRookAttacks = new ulong[64, 4][];
        AllRookAttacks = new ulong[64];

        for (int i = 0; i < 64; i++)
        {
            var rank = 7 - i / 8;
            var file = 7 - i % 8;

            TargetedRookAttacks[i, 0] = new ulong[7 - rank];
            TargetedRookAttacks[i, 1] = new ulong[7 - file];
            TargetedRookAttacks[i, 2] = new ulong[file];
            TargetedRookAttacks[i, 3] = new ulong[rank];

            ulong cell = 1ul << i;

            for (int dist = 1; dist < 8 - rank; dist++)
                TargetedRookAttacks[i, 0][dist - 1] = cell >> (dist * 8);

            for (int dist = 1; dist < 8 - file; dist++)
                TargetedRookAttacks[i, 1][dist - 1] = cell >> dist;

            for (int dist = 1; dist < file + 1; dist++)
                TargetedRookAttacks[i, 2][dist - 1] = cell << dist;

            for (int dist = 1; dist < rank + 1; dist++)
                TargetedRookAttacks[i, 3][dist - 1] = cell << (dist * 8);

            for (int direction = 0; direction < 4; direction++)
            {
                foreach (var c in TargetedRookAttacks[i, direction])
                    AllRookAttacks[i] |= c;
            }
        }
    }

    private static void GenerateBishopAttacks()
    {
        TargetedBishopAttacks = new ulong[64, 4][];
        AllBishopAttacks = new ulong[64];

        for (int i = 0; i < 64; i++)
        {
            var rank = 7 - i / 8;
            var file = 7 - i % 8;

            int above = 7 - rank;
            int below = rank;
            int right = 7 - file;
            int left = file;

            TargetedBishopAttacks[i, 0] = new ulong[Math.Min(above, right)];
            TargetedBishopAttacks[i, 1] = new ulong[Math.Min(above, left)];
            TargetedBishopAttacks[i, 2] = new ulong[Math.Min(below, right)];
            TargetedBishopAttacks[i, 3] = new ulong[Math.Min(below, left)];

            ulong cell = 1ul << i;

            for (int dist = 1; dist < TargetedBishopAttacks[i, 0].Length+1; dist++)
                TargetedBishopAttacks[i, 0][dist - 1] = cell >> (dist * 9);

            for (int dist = 1; dist < TargetedBishopAttacks[i, 1].Length+1; dist++)
                TargetedBishopAttacks[i, 1][dist - 1] = cell >> (dist * 7);

            for (int dist = 1; dist < TargetedBishopAttacks[i, 2].Length+1; dist++)
                TargetedBishopAttacks[i, 2][dist - 1] = cell << (dist * 7);

            for (int dist = 1; dist < TargetedBishopAttacks[i, 3].Length+1; dist++)
                TargetedBishopAttacks[i, 3][dist - 1] = cell << (dist * 9);

            for (int direction = 0; direction < 4; direction++)
            {
                foreach (var c in TargetedBishopAttacks[i, direction])
                    AllBishopAttacks[i] |= c;
            }
        }
    }

    private static void GenerateNeighbours()
    {
        Neighbours = new ulong[64];

        for (int i = 0; i < 64; i++)
        {
            ulong centre = 1ul << i;

            Neighbours[i] = centre;

            bool isOnAFile = (centre & Chessboard.AFile) != 0;
            bool isOnHFile = (centre & Chessboard.HFile) != 0;
            bool isOnFirstRank = (centre & Chessboard.Rank1) != 0;
            bool isOnEighthRank = (centre & Chessboard.Rank8) != 0;

            if (!isOnAFile)
            {
                Neighbours[i] |= centre << 1;

                if (!isOnFirstRank) Neighbours[i] |= centre << 9;
                if (!isOnEighthRank) Neighbours[i] |= centre >> 7;
            }

            if (!isOnHFile)
            {
                Neighbours[i] |= centre >> 1;

                if (!isOnFirstRank) Neighbours[i] |= centre << 7;
                if (!isOnEighthRank) Neighbours[i] |= centre >> 9;
            }

            Neighbours[i] |= centre << 8;
            Neighbours[i] |= centre >> 8;
        }
    }

    public static ulong[] KnightAttacks { get; } = new ulong[64]
    {
        0b00000000_00000000_00000000_00000000_00000000_00000010_00000100_00000000,
        0b00000000_00000000_00000000_00000000_00000000_00000101_00001000_00000000,
        0b00000000_00000000_00000000_00000000_00000000_00001010_00010001_00000000,
        0b00000000_00000000_00000000_00000000_00000000_00010100_00100010_00000000,
        0b00000000_00000000_00000000_00000000_00000000_00101000_01000100_00000000,
        0b00000000_00000000_00000000_00000000_00000000_01010000_10001000_00000000,
        0b00000000_00000000_00000000_00000000_00000000_10100000_00010000_00000000,
        0b00000000_00000000_00000000_00000000_00000000_01000000_00100000_00000000,

        0b00000000_00000000_00000000_00000000_00000010_00000100_00000000_00000100,
        0b00000000_00000000_00000000_00000000_00000101_00001000_00000000_00001000,
        0b00000000_00000000_00000000_00000000_00001010_00010001_00000000_00010001,
        0b00000000_00000000_00000000_00000000_00010100_00100010_00000000_00100010,
        0b00000000_00000000_00000000_00000000_00101000_01000100_00000000_01000100,
        0b00000000_00000000_00000000_00000000_01010000_10001000_00000000_10001000,
        0b00000000_00000000_00000000_00000000_10100000_00010000_00000000_00010000,
        0b00000000_00000000_00000000_00000000_01000000_00100000_00000000_00100000,

        0b00000000_00000000_00000000_00000010_00000100_00000000_00000100_00000010,
        0b00000000_00000000_00000000_00000101_00001000_00000000_00001000_00000101,
        0b00000000_00000000_00000000_00001010_00010001_00000000_00010001_00001010,
        0b00000000_00000000_00000000_00010100_00100010_00000000_00100010_00010100,
        0b00000000_00000000_00000000_00101000_01000100_00000000_01000100_00101000,
        0b00000000_00000000_00000000_01010000_10001000_00000000_10001000_01010000,
        0b00000000_00000000_00000000_10100000_00010000_00000000_00010000_10100000,
        0b00000000_00000000_00000000_01000000_00100000_00000000_00100000_01000000,

        0b00000000_00000000_00000010_00000100_00000000_00000100_00000010_00000000,
        0b00000000_00000000_00000101_00001000_00000000_00001000_00000101_00000000,
        0b00000000_00000000_00001010_00010001_00000000_00010001_00001010_00000000,
        0b00000000_00000000_00010100_00100010_00000000_00100010_00010100_00000000,
        0b00000000_00000000_00101000_01000100_00000000_01000100_00101000_00000000,
        0b00000000_00000000_01010000_10001000_00000000_10001000_01010000_00000000,
        0b00000000_00000000_10100000_00010000_00000000_00010000_10100000_00000000,
        0b00000000_00000000_01000000_00100000_00000000_00100000_01000000_00000000,

        0b00000000_00000010_00000100_00000000_00000100_00000010_00000000_00000000,
        0b00000000_00000101_00001000_00000000_00001000_00000101_00000000_00000000,
        0b00000000_00001010_00010001_00000000_00010001_00001010_00000000_00000000,
        0b00000000_00010100_00100010_00000000_00100010_00010100_00000000_00000000,
        0b00000000_00101000_01000100_00000000_01000100_00101000_00000000_00000000,
        0b00000000_01010000_10001000_00000000_10001000_01010000_00000000_00000000,
        0b00000000_10100000_00010000_00000000_00010000_10100000_00000000_00000000,
        0b00000000_01000000_00100000_00000000_00100000_01000000_00000000_00000000,

        0b00000010_00000100_00000000_00000100_00000010_00000000_00000000_00000000,
        0b00000101_00001000_00000000_00001000_00000101_00000000_00000000_00000000,
        0b00001010_00010001_00000000_00010001_00001010_00000000_00000000_00000000,
        0b00010100_00100010_00000000_00100010_00010100_00000000_00000000_00000000,
        0b00101000_01000100_00000000_01000100_00101000_00000000_00000000_00000000,
        0b01010000_10001000_00000000_10001000_01010000_00000000_00000000_00000000,
        0b10100000_00010000_00000000_00010000_10100000_00000000_00000000_00000000,
        0b01000000_00100000_00000000_00100000_01000000_00000000_00000000_00000000,

        0b00000100_00000000_00000100_00000010_00000000_00000000_00000000_00000000,
        0b00001000_00000000_00001000_00000101_00000000_00000000_00000000_00000000,
        0b00010001_00000000_00010001_00001010_00000000_00000000_00000000_00000000,
        0b00100010_00000000_00100010_00010100_00000000_00000000_00000000_00000000,
        0b01000100_00000000_01000100_00101000_00000000_00000000_00000000_00000000,
        0b10001000_00000000_10001000_01010000_00000000_00000000_00000000_00000000,
        0b00010000_00000000_00010000_10100000_00000000_00000000_00000000_00000000,
        0b00100000_00000000_00100000_01000000_00000000_00000000_00000000_00000000,

        0b00000000_00000100_00000010_00000000_00000000_00000000_00000000_00000000,
        0b00000000_00001000_00000101_00000000_00000000_00000000_00000000_00000000,
        0b00000000_00010001_00001010_00000000_00000000_00000000_00000000_00000000,
        0b00000000_00100010_00010100_00000000_00000000_00000000_00000000_00000000,
        0b00000000_01000100_00101000_00000000_00000000_00000000_00000000_00000000,
        0b00000000_10001000_01010000_00000000_00000000_00000000_00000000_00000000,
        0b00000000_00010000_10100000_00000000_00000000_00000000_00000000_00000000,
        0b00000000_00100000_01000000_00000000_00000000_00000000_00000000_00000000,
    };
}

