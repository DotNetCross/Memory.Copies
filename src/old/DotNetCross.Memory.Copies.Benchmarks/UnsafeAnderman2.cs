using System;
using System.Numerics;

namespace DotNetCross.Memory.Copies.Benchmarks
{
    // https://github.com/dotnet/coreclr/issues/2430#issuecomment-166566393
    public static class UnsafeAnderman2
    {
        /// <summary>
        ///     Copies a specified number of bytes from a source array starting at a particular
        ///     offset to a destination array starting at a particular offset, not safe for overlapping data.
        /// </summary>
        /// <param name="src">The source buffer</param>
        /// <param name="srcOffset">The zero-based byte offset into src</param>
        /// <param name="dst">The destination buffer</param>
        /// <param name="dstOffset">The zero-based byte offset into dst</param>
        /// <param name="count">The number of bytes to copy</param>
        /// <exception cref="ArgumentNullException"><paramref name="src" /> or <paramref name="dst" /> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="srcOffset" />, <paramref name="dstOffset" />, or
        ///     <paramref name="count" /> is less than 0
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The number of bytes in src is less
        ///     than srcOffset plus count.-or- The number of bytes in dst is less than dstOffset
        ///     plus count.
        /// </exception>
        /// <remarks>
        ///     Code must be optimized, in release mode and <see cref="Vector" />.IsHardwareAccelerated must be true for the
        ///     performance benefits.
        /// </remarks>
        public static unsafe void VectorizedCopy2(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
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
                                *(short*) pDst = *(short*) pSrc;
                                *(short*) (pDst + (count & 0xf) - 2) = *(short*) (pSrc + (count & 0xf) - 2);
                            }
                        }
                        else
                        {
                            *(int*) pDst = *(int*) pSrc;
                            *(int*) (pDst + (count & 0xf) - 4) = *(int*) (pSrc + (count & 0xf) - 4);
                        }
                    }
                    else
                    {
                        *(long*) pDst = *(long*) pSrc;
                        *(long*) (pDst + (count & 0xf) - 8) = *(long*) (pSrc + (count & 0xf) - 8);
                    }
                    return;
                }

                pSrc -= (long) pDst;
                Unsafe.Write(pDst, Unsafe.Read<Vector<byte>>(pSrc + (long) pDst));

                var offset = (int) ((ulong) pDst & mask);
                count += offset - alignment;
                pDst += alignment - offset;

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
                while (count >= 2*Vector<byte>.Count)
                {
                    var x1 = Unsafe.Read<Vector<byte>>(pSrc + (long) pDst);
                    var x2 = Unsafe.Read<Vector<byte>>(pSrc + (long) pDst + Vector<byte>.Count);
                    count -= 2*Vector<byte>.Count;
                    Unsafe.Write(pDst, x1);
                    Unsafe.Write(pDst + Vector<byte>.Count, x2);
                    pDst += 2*Vector<byte>.Count;
                }
                while (count >= 1*Vector<byte>.Count)
                {
                    var x1 = Unsafe.Read<Vector<byte>>(pSrc + (long) pDst);
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

        public static unsafe void UnsafeVectorizedCopy2(byte* pDst, byte* pSrc, int count)
        {
            if (count >= Vector<byte>.Count)
            {
                while (count > Vector<byte>.Count)
                {
                    Unsafe.Write(pDst, Unsafe.Read<Vector<byte>>(pSrc));
                    count -= Vector<byte>.Count;
                    pSrc += Vector<byte>.Count;
                    pDst += Vector<byte>.Count;
                }
                pDst += count - Vector<byte>.Count;
                pSrc += count - Vector<byte>.Count;
                Unsafe.Write(pDst, Unsafe.Read<Vector<byte>>(pSrc));
                return;
            }
            switch (count)
            {
                case 15:
                {
                    *(long*) pDst = *(long*) pSrc;
                    *(long*) (pDst + 7) = *(long*) (pSrc + 7);
                    return;
                }
                case 14:
                {
                    *(long*) pDst = *(long*) pSrc;
                    *(long*) (pDst + 6) = *(long*) (pSrc + 6);
                    return;
                }
                case 13:
                {
                    *(long*) pDst = *(long*) pSrc;
                    *(long*) (pDst + 5) = *(long*) (pSrc + 5);
                    return;
                }
                case 12:
                {
                    *(long*) pDst = *(long*) pSrc;
                    *(int*) (pDst + 8) = *(int*) (pSrc + 8);
                    return;
                }
                case 11:
                {
                    *(long*) pDst = *(long*) pSrc;
                    *(int*) (pDst + 7) = *(int*) (pSrc + 7);
                    return;
                }
                case 10:
                {
                    *(long*) pDst = *(long*) pSrc;
                    *(short*) (pDst + 8) = *(short*) (pSrc + 8);
                    return;
                }
                case 9:
                {
                    *(long*) pDst = *(long*) pSrc;
                    *(pDst + 8) = *(pSrc + 8);
                    return;
                }
                case 8:
                {
                    *(long*) pDst = *(long*) pSrc;
                    return;
                }
                case 7:
                {
                    *(int*) pDst = *(int*) pSrc;
                    *(int*) (pDst + 3) = *(int*) (pSrc + 3);
                    return;
                }
                case 6:
                {
                    *(int*) pDst = *(int*) pSrc;
                    *(short*) (pDst + 4) = *(short*) (pSrc + 4);
                    return;
                }
                case 5:
                {
                    *(int*) pDst = *(int*) pSrc;
                    *(pDst + 4) = *(pSrc + 4);
                    return;
                }
                case 4:
                {
                    *(int*) pDst = *(int*) pSrc;
                    return;
                }
                case 3:
                {
                    *(short*) pDst = *(short*) pSrc;
                    *(pDst + 2) = *(pSrc + 2);
                    return;
                }
                case 2:
                {
                    *(short*) pDst = *(short*) pSrc;
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