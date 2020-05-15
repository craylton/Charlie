using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace Charlie
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

        public static ulong CountLeadingZeros(this ulong input)
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

        public static int CountTrailingZeroes(this ulong input)
        {
            return RtlFindLeastSignificantBit(input);
            //return 63 - CountLeadingZeros(input);
        }

        // This dll is apparently a Windows thing
        [DllImport("ntdll"), SuppressUnmanagedCodeSecurity]
        private static extern int RtlFindLeastSignificantBit(ulong ul);

        public static void MoveToFront<T>(this List<T> list, T item)
        {
            if (!list.Contains(item)) return;

            list.Remove(item);
            list.Insert(0, item);
        }
    }
}
