using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DotNetCross.Memory.Copies.Benchmarks
{
    // Based on Anderman primarily https://github.com/dotnet/coreclr/issues/2430#issuecomment-166566393
    public static class UnsafeAnderman2Buffer16
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

            fixed (byte* srcOrigin = &src[srcOffset])
            fixed (byte* dstOrigin = &dst[dstOffset])
            {
                Memmove(dstOrigin, srcOrigin, count);
            }
        }

        public unsafe static void Memmove(byte* pDst, byte* pSrc, int count)
        {
            if (count >= sizeof(Buffer16))
            {
                while (count > sizeof(Buffer16))
                {
                    *((Buffer16*)pDst) = *((Buffer16*)pSrc);
                    count -= sizeof(Buffer16);
                    pSrc += sizeof(Buffer16);
                    pDst += sizeof(Buffer16);
                }
                pDst += count - sizeof(Buffer16);
                pSrc += count - sizeof(Buffer16);
                *((Buffer16*)pDst) = *((Buffer16*)pSrc);
                return;
            }
            switch (count)
            {
                case 15:
                    {
                        *(long*)pDst = *(long*)pSrc;
                        *(long*)(pDst + 7) = *(long*)(pSrc + 7);
                        return;
                    }
                case 14:
                    {
                        *(long*)pDst = *(long*)pSrc;
                        *(long*)(pDst + 6) = *(long*)(pSrc + 6);
                        return;
                    }
                case 13:
                    {
                        *(long*)pDst = *(long*)pSrc;
                        *(long*)(pDst + 5) = *(long*)(pSrc + 5);
                        return;
                    }
                case 12:
                    {
                        *(long*)pDst = *(long*)pSrc;
                        *(int*)(pDst + 8) = *(int*)(pSrc + 8);
                        return;
                    }
                case 11:
                    {
                        *(long*)pDst = *(long*)pSrc;
                        *(int*)(pDst + 7) = *(int*)(pSrc + 7);
                        return;
                    }
                case 10:
                    {
                        *(long*)pDst = *(long*)pSrc;
                        *(short*)(pDst + 8) = *(short*)(pSrc + 8);
                        return;
                    }
                case 9:
                    {
                        *(long*)pDst = *(long*)pSrc;
                        *(pDst + 8) = *(pSrc + 8);
                        return;
                    }
                case 8:
                    {
                        *(long*)pDst = *(long*)pSrc;
                        return;
                    }
                case 7:
                    {
                        *(int*)pDst = *(int*)pSrc;
                        *(int*)(pDst + 3) = *(int*)(pSrc + 3);
                        return;
                    }
                case 6:
                    {
                        *(int*)pDst = *(int*)pSrc;
                        *(short*)(pDst + 4) = *(short*)(pSrc + 4);
                        return;
                    }
                case 5:
                    {
                        *(int*)pDst = *(int*)pSrc;
                        *(pDst + 4) = *(pSrc + 4);
                        return;
                    }
                case 4:
                    {
                        *(int*)pDst = *(int*)pSrc;
                        return;
                    }
                case 3:
                    {
                        *(short*)pDst = *(short*)pSrc;
                        *(pDst + 2) = *(pSrc + 2);
                        return;
                    }
                case 2:
                    {
                        *(short*)pDst = *(short*)pSrc;
                        return;
                    }
                case 1:
                    {
                        *pDst = *pSrc;
                        return;
                    }
                case 0:
                    {
                        return;
                    }
            }
        }
    }
}
