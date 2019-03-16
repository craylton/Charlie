namespace Charlie3
{
    public static class ChessBoard
    {
        public static ulong[] Files { get; } = new ulong[]
        {
            0x80_80_80_80_80_80_80_80,
            0x40_40_40_40_40_40_40_40,
            0x20_20_20_20_20_20_20_20,
            0x10_10_10_10_10_10_10_10,
            0x08_08_08_08_08_08_08_08,
            0x04_04_04_04_04_04_04_04,
            0x02_02_02_02_02_02_02_02,
            0x01_01_01_01_01_01_01_01,
        };

        public readonly static ulong AFile = Files[0];
        public readonly static ulong BFile = Files[1];
        public readonly static ulong CFile = Files[2];
        public readonly static ulong DFile = Files[3];
        public readonly static ulong EFile = Files[4];
        public readonly static ulong FFile = Files[5];
        public readonly static ulong GFile = Files[6];
        public readonly static ulong HFile = Files[7];

        public static ulong[] Ranks { get; } = new ulong[]
        {
            0xFF_00_00_00_00_00_00_00,
            0x00_FF_00_00_00_00_00_00,
            0x00_00_FF_00_00_00_00_00,
            0x00_00_00_FF_00_00_00_00,
            0x00_00_00_00_FF_00_00_00,
            0x00_00_00_00_00_FF_00_00,
            0x00_00_00_00_00_00_FF_00,
            0x00_00_00_00_00_00_00_FF,
        };

        public readonly static ulong Rank1 = Ranks[0];
        public readonly static ulong Rank2 = Ranks[1];
        public readonly static ulong Rank3 = Ranks[2];
        public readonly static ulong Rank4 = Ranks[3];
        public readonly static ulong Rank5 = Ranks[4];
        public readonly static ulong Rank6 = Ranks[5];
        public readonly static ulong Rank7 = Ranks[6];
        public readonly static ulong Rank8 = Ranks[7];
    }
}
