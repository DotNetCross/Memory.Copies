using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DotNetCross.Memory.Copies.Benchmarks2
{
    public static class MsvcrtMemove { 

        public static unsafe void Memmove(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            fixed (byte* pSrcOrigin = &src[srcOffset])
            fixed (byte* pDstOrigin = &dst[dstOffset])
            {
                var pSrc = pSrcOrigin;
                var pDst = pDstOrigin;
                memmove(pDst, pSrc, count);
            }
        }

        [DllImport("msvcrt.dll", SetLastError = false)]
        public static extern unsafe IntPtr memmove(void* dest, void* src, int count);
    }
}