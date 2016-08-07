using System;
using System.Runtime.InteropServices;

namespace DotNetCross.Memory.Copies.Benchmarks
{
    public static class Msvcrt
    {
        public static unsafe void Memmove(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            if (src == null || dst == null) throw new ArgumentNullException(nameof(src));
            if (count < 0 || srcOffset < 0 || dstOffset < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (srcOffset + count > src.Length) throw new ArgumentException(nameof(src));
            if (dstOffset + count > dst.Length) throw new ArgumentException(nameof(dst));

            fixed (byte* srcOrigin = src)
            fixed (byte* dstOrigin = dst)
            {
                var pSrc = srcOrigin + srcOffset;
                var pDst = dstOrigin + dstOffset;

                memmove(pDst, pSrc, count);
            }
        }

        [DllImport("msvcrt.dll", SetLastError = false)]
        public static unsafe extern IntPtr memmove(void* dest, void* src, int count);
    }
}
