namespace Charlie
{
    public static class ULongExtensions
    {
        public static byte BitCount(this ulong value)
        {
            ulong result = value - ((value >> 1) & 0x5555555555555555UL);
            result = (result & 0x3333333333333333UL) + ((result >> 2) & 0x3333333333333333UL);
            return (byte)(unchecked(((result + (result >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
        }

        public static int CountLeadingZeros(this ulong input)
        {
            if (input == 0) return 0;

            ulong n = 1;

            if ((input >> 32) == 0) { n += 32; input <<= 32; }
            if ((input >> 48) == 0) { n += 16; input <<= 16; }
            if ((input >> 56) == 0) { n += 8; input <<= 8; }
            if ((input >> 60) == 0) { n += 4; input <<= 4; }
            if ((input >> 62) == 0) { n += 2; input <<= 2; }
            n -= input >> 63;

            return (int)n;
        }

        public static int CountTrailingZeroes(this ulong input) => 63 - CountLeadingZeros(input);
    }
}
