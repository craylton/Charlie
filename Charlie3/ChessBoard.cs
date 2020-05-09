namespace Charlie3
{
    public static class ChessBoard
    {
        public static ulong[] Files { get; } = new ulong[]
        {
            AFile,
            BFile,
            CFile,
            DFile,
            EFile,
            FFile,
            GFile,
            HFile,
        };

        public const ulong AFile = 0x80_80_80_80_80_80_80_80;
        public const ulong BFile = 0x40_40_40_40_40_40_40_40;
        public const ulong CFile = 0x20_20_20_20_20_20_20_20;
        public const ulong DFile = 0x10_10_10_10_10_10_10_10;
        public const ulong EFile = 0x08_08_08_08_08_08_08_08;
        public const ulong FFile = 0x04_04_04_04_04_04_04_04;
        public const ulong GFile = 0x02_02_02_02_02_02_02_02;
        public const ulong HFile = 0x01_01_01_01_01_01_01_01;

        public static ulong[] Ranks { get; } = new ulong[]
        {
            Rank1,
            Rank2,
            Rank3,
            Rank4,
            Rank5,
            Rank6,
            Rank7,
            Rank8,
        };

        public const ulong Rank1 = 0xFF_00_00_00_00_00_00_00;
        public const ulong Rank2 = 0x00_FF_00_00_00_00_00_00;
        public const ulong Rank3 = 0x00_00_FF_00_00_00_00_00;
        public const ulong Rank4 = 0x00_00_00_FF_00_00_00_00;
        public const ulong Rank5 = 0x00_00_00_00_FF_00_00_00;
        public const ulong Rank6 = 0x00_00_00_00_00_FF_00_00;
        public const ulong Rank7 = 0x00_00_00_00_00_00_FF_00;
        public const ulong Rank8 = 0x00_00_00_00_00_00_00_FF;

        public const ulong SquareA1 = Rank1 & AFile;
        public const ulong SquareA2 = Rank2 & AFile;
        public const ulong SquareA3 = Rank3 & AFile;
        public const ulong SquareA4 = Rank4 & AFile;
        public const ulong SquareA5 = Rank5 & AFile;
        public const ulong SquareA6 = Rank6 & AFile;
        public const ulong SquareA7 = Rank7 & AFile;
        public const ulong SquareA8 = Rank8 & AFile;

        public const ulong SquareB1 = Rank1 & BFile;
        public const ulong SquareB2 = Rank2 & BFile;
        public const ulong SquareB3 = Rank3 & BFile;
        public const ulong SquareB4 = Rank4 & BFile;
        public const ulong SquareB5 = Rank5 & BFile;
        public const ulong SquareB6 = Rank6 & BFile;
        public const ulong SquareB7 = Rank7 & BFile;
        public const ulong SquareB8 = Rank8 & BFile;

        public const ulong SquareC1 = Rank1 & CFile;
        public const ulong SquareC2 = Rank2 & CFile;
        public const ulong SquareC3 = Rank3 & CFile;
        public const ulong SquareC4 = Rank4 & CFile;
        public const ulong SquareC5 = Rank5 & CFile;
        public const ulong SquareC6 = Rank6 & CFile;
        public const ulong SquareC7 = Rank7 & CFile;
        public const ulong SquareC8 = Rank8 & CFile;

        public const ulong SquareD1 = Rank1 & DFile;
        public const ulong SquareD2 = Rank2 & DFile;
        public const ulong SquareD3 = Rank3 & DFile;
        public const ulong SquareD4 = Rank4 & DFile;
        public const ulong SquareD5 = Rank5 & DFile;
        public const ulong SquareD6 = Rank6 & DFile;
        public const ulong SquareD7 = Rank7 & DFile;
        public const ulong SquareD8 = Rank8 & DFile;

        public const ulong SquareE1 = Rank1 & EFile;
        public const ulong SquareE2 = Rank2 & EFile;
        public const ulong SquareE3 = Rank3 & EFile;
        public const ulong SquareE4 = Rank4 & EFile;
        public const ulong SquareE5 = Rank5 & EFile;
        public const ulong SquareE6 = Rank6 & EFile;
        public const ulong SquareE7 = Rank7 & EFile;
        public const ulong SquareE8 = Rank8 & EFile;

        public const ulong SquareF1 = Rank1 & FFile;
        public const ulong SquareF2 = Rank2 & FFile;
        public const ulong SquareF3 = Rank3 & FFile;
        public const ulong SquareF4 = Rank4 & FFile;
        public const ulong SquareF5 = Rank5 & FFile;
        public const ulong SquareF6 = Rank6 & FFile;
        public const ulong SquareF7 = Rank7 & FFile;
        public const ulong SquareF8 = Rank8 & FFile;

        public const ulong SquareG1 = Rank1 & GFile;
        public const ulong SquareG2 = Rank2 & GFile;
        public const ulong SquareG3 = Rank3 & GFile;
        public const ulong SquareG4 = Rank4 & GFile;
        public const ulong SquareG5 = Rank5 & GFile;
        public const ulong SquareG6 = Rank6 & GFile;
        public const ulong SquareG7 = Rank7 & GFile;
        public const ulong SquareG8 = Rank8 & GFile;

        public const ulong SquareH1 = Rank1 & HFile;
        public const ulong SquareH2 = Rank2 & HFile;
        public const ulong SquareH3 = Rank3 & HFile;
        public const ulong SquareH4 = Rank4 & HFile;
        public const ulong SquareH5 = Rank5 & HFile;
        public const ulong SquareH6 = Rank6 & HFile;
        public const ulong SquareH7 = Rank7 & HFile;
        public const ulong SquareH8 = Rank8 & HFile;
    }
}
