using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace test
{
    public static class Anderman2 { 
        public static unsafe void VectorizedCopyAnderman(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            const int alignment = 0x10;
            const int mask = alignment - 1;
            if (src == null || dst == null) throw new ArgumentNullException(nameof(src));
            if (srcOffset + count > src.Length) throw new ArgumentException(nameof(src));
            if (count < 0 || srcOffset < 0 || dstOffset < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (dstOffset + count > dst.Length) throw new ArgumentException(nameof(dst));
            if (count == 0) return;
            fixed (byte* pSrcOrigin = &src[srcOffset])
            fixed (byte* pDstOrigin = &dst[dstOffset])
            {
                var pSrc = pSrcOrigin;
                var pDst = pDstOrigin;
                if (count >= 8)
                {
                    *(long*) pDst = *(long*) pSrc;
                    *(long*) (pDst + (count & 0xf) - 8) = *(long*) (pSrc + (count & 0xf) - 8);
                }
                else if (count >= 4)
                {
                    *(int*) pDst = *(int*) pSrc;
                    *(int*) (pDst + (count & 0xf) - 4) = *(int*) (pSrc + (count & 0xf) - 4);
                }
                else if (count >= 2)
                {
                    *(short*) pDst = *(short*) pSrc;
                    *(short*) (pDst + (count & 0xf) - 2) = *(short*) (pSrc + (count & 0xf) - 2);
                }
                else
                {
                    *(pDst + 0) = *(pSrc + 0);
                }
                if (count >= Vector<byte>.Count)
                {
                    pSrc -= (long) pDst;
                    count += -alignment + (int) ((ulong) pDst & mask);
                    pDst += alignment - ((ulong) pDst & mask);

                    while (count >= 4*Vector<byte>.Count)
                    {
                        var x1 = Unsafe.Read<Vector<byte>>(pSrc + (long) pDst);
                        var x2 = Unsafe.Read<Vector<byte>>(pSrc + (long) pDst + Vector<byte>.Count);
                        count -= 4*Vector<byte>.Count;
                        Unsafe.Write(pDst, x1);
                        Unsafe.Write(pDst + Vector<byte>.Count, x2);
                        pDst += 4*Vector<byte>.Count;
                        x1 = Unsafe.Read<Vector<byte>>(pSrc + (long) pDst - 2*Vector<byte>.Count);
                        x2 = Unsafe.Read<Vector<byte>>(pSrc + (long) pDst - Vector<byte>.Count);
                        Unsafe.Write(pDst - 2*Vector<byte>.Count, x1);
                        Unsafe.Write(pDst - Vector<byte>.Count, x2);
                    }
                    while (count >= 1*Vector<byte>.Count)
                    {
                        var x1 = Unsafe.Read<Vector<byte>>(pSrc + (long) pDst);
                        count -= Vector<byte>.Count;
                        Unsafe.Write(pDst, x1);
                        pDst += Vector<byte>.Count;
                    }
                    pDst += count - Vector<byte>.Count;
                    Unsafe.Write(pDst, Unsafe.Read<Vector<byte>>(pSrc + (long) pDst));
                }
            }
        }
    }
}