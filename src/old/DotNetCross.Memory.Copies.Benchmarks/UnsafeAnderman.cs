using System;
using System.Numerics;

namespace DotNetCross.Memory.Copies.Benchmarks
{
    // https://github.com/dotnet/coreclr/issues/2430#issuecomment-166566393
    public static class UnsafeAnderman
    {
        /// <summary>
        /// Copies a specified number of bytes from a source array starting at a particular
        /// offset to a destination array starting at a particular offset, not safe for overlapping data.
        /// </summary>
        /// <param name="src">The source buffer</param>
        /// <param name="srcOffset">The zero-based byte offset into src</param>
        /// <param name="dst">The destination buffer</param>
        /// <param name="dstOffset">The zero-based byte offset into dst</param>
        /// <param name="count">The number of bytes to copy</param>
        /// <exception cref="ArgumentNullException"><paramref name="src"/> or <paramref name="dst"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="srcOffset"/>, <paramref name="dstOffset"/>, or <paramref name="count"/> is less than 0</exception>
        /// <exception cref="ArgumentException">
        /// The number of bytes in src is less
        /// than srcOffset plus count.-or- The number of bytes in dst is less than dstOffset
        /// plus count.
        /// </exception>
        /// <remarks>
        /// Code must be optimized, in release mode and <see cref="Vector"/>.IsHardwareAccelerated must be true for the performance benefits.
        /// </remarks>
        public static unsafe void VectorizedCopy(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
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
                var pSrc = srcOrigin + srcOffset;
                var pDst = dstOrigin + dstOffset;
                while (count >= Vector<byte>.Count)
                {
                    Unsafe.Write(pDst, Unsafe.Read<Vector<byte>>(pSrc));
                    count -= Vector<byte>.Count;
                    pSrc += Vector<byte>.Count;
                    pDst += Vector<byte>.Count;
                }
                if (orgCount > Vector<byte>.Count)
                {
                    // Is this right? What about offset?
                    //new Vector<byte>(src, orgCount - Vector<byte>.Count).CopyTo(dst, orgCount - Vector<byte>.Count);
                    var offset = orgCount - Vector<byte>.Count;
                    Unsafe.Write(dstOrigin + offset, Unsafe.Read<Vector<byte>>(srcOrigin + offset));
                    return;
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
        public unsafe static void UnsafeVectorizedCopy2(byte* pDst, byte* pSrc, int count)
        {
            while (count >= Vector<byte>.Count)
            {
                Unsafe.Write(pDst, Unsafe.Read<Vector<byte>>(pSrc));
                count -= Vector<byte>.Count;
                pSrc += Vector<byte>.Count;
                pDst += Vector<byte>.Count;
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
