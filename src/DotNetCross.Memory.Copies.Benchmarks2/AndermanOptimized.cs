using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Memory.Copies.Benchmarks2
{
    public static class AndermanOptimized
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Memmove(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            const int alignment = 0x10;
            const int mask = alignment - 1;

            if (count == 0) return;
            if (src == null || dst == null) throw new ArgumentNullException(nameof(src));
            if (srcOffset + count > src.Length) throw new ArgumentException(nameof(src));
            if (count < 0 || srcOffset < 0 || dstOffset < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (dstOffset + count > dst.Length) throw new ArgumentException(nameof(dst));

            fixed (byte* pSrcOrigin = &src[srcOffset])
            fixed (byte* pDstOrigin = &dst[dstOffset])
            {
                var pSrc = pSrcOrigin;
                var pDst = pDstOrigin;
                if (count < 16)
                {
                    if (count < 8)
                    {
                        if (count < 4)
                        {
                            if (count < 2)
                            {
                                *(pDst + 0) = *(pSrc + 0);
                            }
                            else
                            {
                                *(short*)pDst = *(short*)pSrc;
                                *(short*)(pDst + (count & 0xf) - 2) = *(short*)(pSrc + (count & 0xf) - 2);
                            }
                        }
                        else
                        {
                            *(int*)pDst = *(int*)pSrc;
                            *(int*)(pDst + (count & 0xf) - 4) = *(int*)(pSrc + (count & 0xf) - 4);
                        }

                    }
                    else
                    {
                        *(long*)pDst = *(long*)pSrc;
                        *(long*)(pDst + (count & 0xf) - 8) = *(long*)(pSrc + (count & 0xf) - 8);
                    }
                    return;
                }

                pSrc -= (long)pDst;
                Unsafe.Write(pDst, Unsafe.Read<Vector<byte>>(pSrc + (long)pDst));

                var offset = (int)((ulong)pDst & mask);
                count += offset - alignment;
                pDst += alignment - offset;

                while (count >= 4 * Vector<byte>.Count)
                {
                    var x1 = Unsafe.Read<Vector<byte>>(pSrc + (long)pDst);
                    var x2 = Unsafe.Read<Vector<byte>>(pSrc + (long)pDst + Vector<byte>.Count);
                    count -= 4 * Vector<byte>.Count;
                    Unsafe.Write(pDst, x1);
                    Unsafe.Write(pDst + Vector<byte>.Count, x2);
                    pDst += 4 * Vector<byte>.Count;
                    x1 = Unsafe.Read<Vector<byte>>(pSrc + (long)pDst - 2 * Vector<byte>.Count);
                    x2 = Unsafe.Read<Vector<byte>>(pSrc + (long)pDst - Vector<byte>.Count);
                    Unsafe.Write(pDst - 2 * Vector<byte>.Count, x1);
                    Unsafe.Write(pDst - Vector<byte>.Count, x2);
                }
                while (count >= 2 * Vector<byte>.Count)
                {
                    var x1 = Unsafe.Read<Vector<byte>>(pSrc + (long)pDst);
                    var x2 = Unsafe.Read<Vector<byte>>(pSrc + (long)pDst + Vector<byte>.Count);
                    count -= 2 * Vector<byte>.Count;
                    Unsafe.Write(pDst, x1);
                    Unsafe.Write(pDst + Vector<byte>.Count, x2);
                    pDst += 2 * Vector<byte>.Count;
                }
                while (count >= 1 * Vector<byte>.Count)
                {
                    var x1 = Unsafe.Read<Vector<byte>>(pSrc + (long)pDst);
                    count -= Vector<byte>.Count;
                    Unsafe.Write(pDst, x1);
                    pDst += Vector<byte>.Count;
                }
                if (count > 0)
                {
                    pDst += count - Vector<byte>.Count;
                    Unsafe.Write(pDst, Unsafe.Read<Vector<byte>>(pSrc + (long) pDst));
                }
            }
        }
    }

}