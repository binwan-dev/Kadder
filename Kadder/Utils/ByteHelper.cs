using System;

namespace Kadder.Utils
{
    public static class ByteHelper
    {
        public static byte SetByte(this byte data, int index, bool flag)
        {
            if (index > 8 || index < 1)
                throw new ArgumentOutOfRangeException();
            int v = index < 2 ? index : (2 << (index - 2));
            return flag ? (byte) (data | v) : (byte) (data & ~v);
        }

        public static bool GetBinary(this byte data, int index)
        {
            return ((data >> index) & 0x1) == 1;
        }
    }
}