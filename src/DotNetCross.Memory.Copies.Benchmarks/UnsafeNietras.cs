using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DotNetCross.Memory.Copies.Benchmarks
{
    // Based on Anderman primarily https://github.com/dotnet/coreclr/issues/2430#issuecomment-166566393
    public static class UnsafeNietras
    {
        [StructLayout(LayoutKind.Sequential, Size = 16)]
        private struct Buffer16
        {
            //ulong l1; ulong l2;
        }

        [StructLayout(LayoutKind.Sequential, Size = 32)]
        private struct Buffer32
        {
            //ulong l1; ulong l2; ulong l3; ulong l4;
        }
        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct Buffer64
        {
            //ulong l1; ulong l2; ulong l3; ulong l4; ulong l5; ulong l6; ulong l7; ulong l8;
        }


        public static unsafe void Memmove(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            if (count > 512 + 64)
            {
                // TEST: Disable Array-Copy fall back
                // In-built copy faster for large arrays (vs repeated bounds checks on Vector.ctor?)
                //Array.Copy(src, srcOffset, dst, dstOffset, count);
                //return;
            }
            var orgCount = count;

            if (src == null || dst == null) throw new ArgumentNullException(nameof(src));
            if (count < 0 || srcOffset < 0 || dstOffset < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (srcOffset + count > src.Length) throw new ArgumentException(nameof(src));
            if (dstOffset + count > dst.Length) throw new ArgumentException(nameof(dst));

            fixed (byte* srcOrigin = src)
            fixed (byte* dstOrigin = dst)
            {
                Memmove(dstOrigin, srcOrigin, count);
            }
        }
        public unsafe static void Memmove(byte* pDst, byte* pSrc, int count)
        {
            while (count >= sizeof(Buffer16))
            {
                *((Buffer16*)pDst) = *((Buffer16*)pSrc);
                count -= sizeof(Buffer16);
                pSrc += sizeof(Buffer16);
                pDst += sizeof(Buffer16);
            }
            switch (count)
            {
                case 1:
                    pDst[0] = pSrc[0];
                    return;

                case 2:
                    *((short*)pDst) = *((short*)pSrc);
                    return;

                case 3:
                    *((short*)pDst) = *((short*)pSrc);
                    pDst[2] = pSrc[2];
                    return;

                case 4:
                    *((int*)pDst) = *((int*)pSrc);
                    return;

                case 5:
                    *((int*)pDst) = *((int*)pSrc);
                    pDst[4] = pSrc[4];
                    return;

                case 6:
                    *((int*)pDst) = *((int*)pSrc);
                    *((short*)(pDst + 4)) = *((short*)(pSrc + 4));
                    return;

                case 7:
                    *((int*)pDst) = *((int*)pSrc);
                    *((short*)(pDst + 4)) = *((short*)(pSrc + 4));
                    pDst[6] = pSrc[6];
                    return;

                case 8:
                    *((long*)pDst) = *((long*)pSrc);
                    return;

                case 9:
                    *((long*)pDst) = *((long*)pSrc);
                    pDst[8] = pSrc[8];
                    return;

                case 10:
                    *((long*)pDst) = *((long*)pSrc);
                    *((short*)(pDst + 8)) = *((short*)(pSrc + 8));
                    return;

                case 11:
                    *((long*)pDst) = *((long*)pSrc);
                    *((short*)(pDst + 8)) = *((short*)(pSrc + 8));
                    pDst[10] = pSrc[10];
                    return;

                case 12:
                    *((long*)pDst) = *((long*)pSrc);
                    *((int*)(pDst + 8)) = *((int*)(pSrc + 8));
                    return;

                case 13:
                    *((long*)pDst) = *((long*)pSrc);
                    *((int*)(pDst + 8)) = *((int*)(pSrc + 8));
                    pDst[12] = pSrc[12];
                    return;

                case 14:
                    *((long*)pDst) = *((long*)pSrc);
                    *((int*)(pDst + 8)) = *((int*)(pSrc + 8));
                    *((short*)(pDst + 12)) = *((short*)(pSrc + 12));
                    return;

                case 15:
                    *((long*)pDst) = *((long*)pSrc);
                    *((int*)(pDst + 8)) = *((int*)(pSrc + 8));
                    *((short*)(pDst + 12)) = *((short*)(pSrc + 12));
                    pDst[14] = pSrc[14];
                    return;

            }
        }
    }
}
