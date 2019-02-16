namespace Charlie3
{
    public static class Extensions
    {    
        /// <summary>
         /// Count the number of bits set to 1 in a ulong
         /// </summary>
        public static byte BitCount(this ulong value)
        {
            ulong result = value - ((value >> 1) & 0x5555555555555555UL);
            result = (result & 0x3333333333333333UL) + ((result >> 2) & 0x3333333333333333UL);
            return (byte)(unchecked(((result + (result >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
        }
    }
}
