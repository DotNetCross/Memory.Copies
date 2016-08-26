using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DotNetCross.Memory.Copies.Benchmarks2
{
    public static class MsvcrtMemove { 

        public static unsafe void Memmove(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            fixed (byte* srcOrigin = src)
            fixed (byte* dstOrigin = dst)
            {
                var pSrc = srcOrigin + srcOffset;
                var pDst = dstOrigin + dstOffset;

                memmove(pDst, pSrc, count);
            }
        }

        [DllImport("msvcrt.dll", SetLastError = false)]
        public static extern unsafe IntPtr memmove(void* dest, void* src, int count);
    }
}