using System;
using System.Numerics;

namespace DotNetCross.Memory.Copies.Benchmarks
{
    // https://github.com/IllyriadGames/ByteArrayExtensions/blob/master/src/IllyriadGames.ByteArrayExtensions/VectorizedCopyExtension.cs
    public static class IllyriadVectorizedCopy
    {
        // Will be Jit'd to consts https://github.com/dotnet/coreclr/issues/1079
        private static readonly int _vectorSpan = Vector<byte>.Count;
        private static readonly int _vectorSpan2 = Vector<byte>.Count + Vector<byte>.Count;
        private static readonly int _vectorSpan3 = Vector<byte>.Count + Vector<byte>.Count + Vector<byte>.Count;
        private static readonly int _vectorSpan4 = Vector<byte>.Count + Vector<byte>.Count + Vector<byte>.Count + Vector<byte>.Count;

        private const int _longSpan = sizeof(long);
        private const int _longSpan2 = sizeof(long) + sizeof(long);
        private const int _longSpan3 = sizeof(long) + sizeof(long) + sizeof(long);
        private const int _intSpan = sizeof(int);

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
        public unsafe static void VectorizedCopy(this byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
#if !DEBUG
            // Tests need to check even if IsHardwareAccelerated == false
            // Check will be Jitted away https://github.com/dotnet/coreclr/issues/1079
            if (Vector.IsHardwareAccelerated)
            {
#endif
            if (count > 512 + 64)
            {
                // In-built copy faster for large arrays (vs repeated bounds checks on Vector.ctor?)
                Array.Copy(src, srcOffset, dst, dstOffset, count);
                return;
            }
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (dst == null) throw new ArgumentNullException(nameof(dst));
            if (count < 0 || srcOffset < 0 || dstOffset < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return;
            if (srcOffset + count > src.Length) throw new ArgumentException(nameof(src));
            if (dstOffset + count > dst.Length) throw new ArgumentException(nameof(dst));

            while (count >= _vectorSpan4)
            {
                new Vector<byte>(src, srcOffset).CopyTo(dst, dstOffset);
                new Vector<byte>(src, srcOffset + _vectorSpan).CopyTo(dst, dstOffset + _vectorSpan);
                new Vector<byte>(src, srcOffset + _vectorSpan2).CopyTo(dst, dstOffset + _vectorSpan2);
                new Vector<byte>(src, srcOffset + _vectorSpan3).CopyTo(dst, dstOffset + _vectorSpan3);
                if (count == _vectorSpan4) return;
                count -= _vectorSpan4;
                srcOffset += _vectorSpan4;
                dstOffset += _vectorSpan4;
            }
            if (count >= _vectorSpan2)
            {
                new Vector<byte>(src, srcOffset).CopyTo(dst, dstOffset);
                new Vector<byte>(src, srcOffset + _vectorSpan).CopyTo(dst, dstOffset + _vectorSpan);
                if (count == _vectorSpan2) return;
                count -= _vectorSpan2;
                srcOffset += _vectorSpan2;
                dstOffset += _vectorSpan2;
            }
            if (count >= _vectorSpan)
            {
                new Vector<byte>(src, srcOffset).CopyTo(dst, dstOffset);
                if (count == _vectorSpan) return;
                count -= _vectorSpan;
                srcOffset += _vectorSpan;
                dstOffset += _vectorSpan;
            }
            if (count > 0)
            {
                fixed (byte* srcOrigin = src)
                fixed (byte* dstOrigin = dst)
                {
                    var pSrc = srcOrigin + srcOffset;
                    var dSrc = dstOrigin + dstOffset;

                    if (count >= _longSpan)
                    {
                        var lpSrc = (long*)pSrc;
                        var ldSrc = (long*)dSrc;

                        if (count < _longSpan2)
                        {
                            count -= _longSpan;
                            pSrc += _longSpan;
                            dSrc += _longSpan;
                            *ldSrc = *lpSrc;
                        }
                        else if (count < _longSpan3)
                        {
                            count -= _longSpan2;
                            pSrc += _longSpan2;
                            dSrc += _longSpan2;
                            *ldSrc = *lpSrc;
                            *(ldSrc + 1) = *(lpSrc + 1);
                        }
                        else
                        {
                            count -= _longSpan3;
                            pSrc += _longSpan3;
                            dSrc += _longSpan3;
                            *ldSrc = *lpSrc;
                            *(ldSrc + 1) = *(lpSrc + 1);
                            *(ldSrc + 2) = *(lpSrc + 2);
                        }
                    }
                    if (count >= _intSpan)
                    {
                        var ipSrc = (int*)pSrc;
                        var idSrc = (int*)dSrc;
                        count -= _intSpan;
                        pSrc += _intSpan;
                        dSrc += _intSpan;
                        *idSrc = *ipSrc;
                    }
                    while (count > 0)
                    {
                        count--;
                        *dSrc = *pSrc;
                        dSrc += 1;
                        pSrc += 1;
                    }
                }
            }
#if !DEBUG
            }
            else
            {
                Array.Copy(src, srcOffset, dst, dstOffset, count);
                return;
            }
#endif
        }
    }
}
