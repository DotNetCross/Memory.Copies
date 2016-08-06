using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCross.Memory.Copies.Benchmarks
{
    // jamesqo, tannergooding optimized https://github.com/dotnet/coreclr/pull/6638 and https://github.com/dotnet/coreclr/pull/6627
    public class UnsafeBufferMemmoveTannerGooding
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

                Memmove(pDst, pSrc, (uint)count);
            }
        }

        internal unsafe static void Memmove(byte* dst, byte* src, uint len)
        {
            switch (len)
            {
                case 0:
                    {
                        return;
                    }

                case 1:
                    {
                        *dst = *src;
                        return;
                    }

                case 2:
                    {
                        *(ushort*)(dst) = *(ushort*)(src);
                        return;
                    }

                case 3:
                    {
                        *(ushort*)(dst) = *(ushort*)(src);
                        *(dst + sizeof(ushort)) = *(src + sizeof(ushort));
                        return;
                    }

                case 4:
                    {
                        *(uint*)(dst) = *(uint*)(src);
                        return;
                    }

                case 5:
                    {
                        *(uint*)(dst) = *(uint*)(src);
                        *(dst + sizeof(uint)) = *(src + sizeof(uint));
                        return;
                    }

                case 6:
                    {
                        *(uint*)(dst) = *(uint*)(src);
                        *(ushort*)(dst + sizeof(uint)) = *(ushort*)(src + sizeof(uint));
                        return;
                    }

                case 7:
                    {
                        *(uint*)(dst) = *(uint*)(src);
                        *(ushort*)(dst + sizeof(uint)) = *(ushort*)(src + sizeof(uint));
                        *(dst + sizeof(uint) + sizeof(ushort)) = *(src + sizeof(uint) + sizeof(ushort));
                        return;
                    }

                case 8:
                    {
                        *(ulong*)(dst) = *(ulong*)(src);
                        return;
                    }

                case 9:
                    {
                        *(ulong*)(dst) = *(ulong*)(src);
                        *(dst + sizeof(ulong)) = *(src + sizeof(ulong));
                        return;
                    }

                case 10:
                    {
                        *(ulong*)(dst) = *(ulong*)(src);
                        *(ushort*)(dst + sizeof(ulong)) = *(ushort*)(src + sizeof(ulong));
                        return;
                    }

                case 11:
                    {
                        *(ulong*)(dst) = *(ulong*)(src);
                        *(ushort*)(dst + sizeof(ulong)) = *(ushort*)(src + sizeof(ulong));
                        *(dst + sizeof(ulong) + sizeof(ushort)) = *(src + sizeof(ulong) + sizeof(ushort));
                        return;
                    }

                case 12:
                    {
                        *(ulong*)(dst) = *(ulong*)(src);
                        *(uint*)(dst + sizeof(ulong)) = *(uint*)(src + sizeof(ulong));
                        return;
                    }

                case 13:
                    {
                        *(ulong*)(dst) = *(ulong*)(src);
                        *(uint*)(dst + sizeof(ulong)) = *(uint*)(src + sizeof(ulong));
                        *(dst + sizeof(ulong) + sizeof(uint)) = *(src + sizeof(ulong) + sizeof(uint));
                        return;
                    }

                case 14:
                    {
                        *(ulong*)(dst) = *(ulong*)(src);
                        *(uint*)(dst + sizeof(ulong)) = *(uint*)(src + sizeof(ulong));
                        *(ushort*)(dst + sizeof(ulong) + sizeof(uint)) = *(ushort*)(src + sizeof(ulong) + sizeof(uint));
                        return;
                    }

                case 15:
                    {
                        *(ulong*)(dst) = *(ulong*)(src);
                        *(uint*)(dst + sizeof(ulong)) = *(uint*)(src + sizeof(ulong));
                        *(ushort*)(dst + sizeof(ulong) + sizeof(uint)) = *(ushort*)(src + sizeof(ulong) + sizeof(uint));
                        *(dst + sizeof(ulong) + sizeof(uint) + sizeof(ushort)) = *(src + sizeof(ulong) + sizeof(uint) + sizeof(ushort));
                        return;
                    }

                case 16:
                    {
                        *(decimal*)(dst) = *(decimal*)(src);
                        return;
                    }
            }

            if (len <= 32)
            {
                // We can do this in two writes. Note that one or both of these writes may be misaligned

                *(decimal*)(dst) = *(decimal*)(src);
                *(decimal*)(dst + len - sizeof(decimal)) = *(decimal*)(src + len - sizeof(decimal));

                return;
            }

            if ((src < dst) && ((src + len) > dst))
            {
                // (src < dest) && ((src + len) > dst), so we need to P/Invoke
                // _Memmove(dest, src, len);
            }

            var misalignment = ((uint)(dst) % sizeof(decimal));

            if (misalignment != 0)
            {
                *(decimal*)(dst) = *(decimal*)(src);
                *(decimal*)(dst + misalignment) = *(decimal*)(src + misalignment);

                var initialOffset = (sizeof(decimal) + misalignment);

                len -= initialOffset;
                src += initialOffset;
                dst += initialOffset;
            }

            const uint blockSize = (sizeof(decimal) * 8);

            if (len > blockSize)
            {
                var iterations = (len / blockSize);

                for (var iteration = 0ul; iteration < iterations; iteration++)
                {
                    *(decimal*)(dst) = *(decimal*)(src);
                    *(decimal*)(dst + sizeof(decimal)) = *(decimal*)(src + sizeof(decimal));
                    *(decimal*)(dst + (sizeof(decimal) * 2)) = *(decimal*)(src + (sizeof(decimal) * 2));
                    *(decimal*)(dst + (sizeof(decimal) * 3)) = *(decimal*)(src + (sizeof(decimal) * 3));

                    *(decimal*)(dst + (sizeof(decimal) * 4)) = *(decimal*)(src + (sizeof(decimal) * 4));
                    *(decimal*)(dst + (sizeof(decimal) * 5)) = *(decimal*)(src + (sizeof(decimal) * 5));
                    *(decimal*)(dst + (sizeof(decimal) * 6)) = *(decimal*)(src + (sizeof(decimal) * 6));
                    *(decimal*)(dst + (sizeof(decimal) * 7)) = *(decimal*)(src + (sizeof(decimal) * 7));

                    src += blockSize;
                    dst += blockSize;
                }

                len -= (iterations * blockSize);
            }

            if (len == 0)
            {
                return;
            }

            var remainingBlocks = (len / sizeof(decimal));
            var remainingBytes = (len - (remainingBlocks * sizeof(decimal)));

            switch (remainingBlocks)
            {
                case 0:
                    {
                        *(decimal*)(dst - sizeof(decimal) + remainingBytes) = *(decimal*)(src - sizeof(decimal) + remainingBytes);
                        return;
                    }

                case 1:
                    {
                        *(decimal*)(dst) = *(decimal*)(src);
                        *(decimal*)(dst + remainingBytes) = *(decimal*)(src + remainingBytes);
                        return;
                    }

                case 2:
                    {
                        *(decimal*)(dst) = *(decimal*)(src);
                        *(decimal*)(dst + sizeof(decimal)) = *(decimal*)(src + sizeof(decimal));
                        *(decimal*)(dst + sizeof(decimal) + remainingBytes) = *(decimal*)(src + sizeof(decimal) + remainingBytes);
                        return;
                    }

                case 3:
                    {
                        *(decimal*)(dst) = *(decimal*)(src);
                        *(decimal*)(dst + sizeof(decimal)) = *(decimal*)(src + sizeof(decimal));
                        *(decimal*)(dst + (sizeof(decimal) * 2)) = *(decimal*)(src + (sizeof(decimal) * 2));
                        *(decimal*)(dst + (sizeof(decimal) * 2) + remainingBytes) = *(decimal*)(src + (sizeof(decimal) * 2) + remainingBytes);
                        return;
                    }

                case 4:
                    {
                        *(decimal*)(dst) = *(decimal*)(src);
                        *(decimal*)(dst + sizeof(decimal)) = *(decimal*)(src + sizeof(decimal));
                        *(decimal*)(dst + (sizeof(decimal) * 2)) = *(decimal*)(src + (sizeof(decimal) * 2));
                        *(decimal*)(dst + (sizeof(decimal) * 3)) = *(decimal*)(src + (sizeof(decimal) * 3));
                        *(decimal*)(dst + (sizeof(decimal) * 3) + remainingBytes) = *(decimal*)(src + (sizeof(decimal) * 3) + remainingBytes);
                        return;
                    }

                case 5:
                    {
                        *(decimal*)(dst) = *(decimal*)(src);
                        *(decimal*)(dst + sizeof(decimal)) = *(decimal*)(src + sizeof(decimal));
                        *(decimal*)(dst + (sizeof(decimal) * 2)) = *(decimal*)(src + (sizeof(decimal) * 2));
                        *(decimal*)(dst + (sizeof(decimal) * 3)) = *(decimal*)(src + (sizeof(decimal) * 3));
                        *(decimal*)(dst + (sizeof(decimal) * 4)) = *(decimal*)(src + (sizeof(decimal) * 4));
                        *(decimal*)(dst + (sizeof(decimal) * 4) + remainingBytes) = *(decimal*)(src + (sizeof(decimal) * 4) + remainingBytes);
                        return;
                    }

                case 6:
                    {
                        *(decimal*)(dst) = *(decimal*)(src);
                        *(decimal*)(dst + sizeof(decimal)) = *(decimal*)(src + sizeof(decimal));
                        *(decimal*)(dst + (sizeof(decimal) * 2)) = *(decimal*)(src + (sizeof(decimal) * 2));
                        *(decimal*)(dst + (sizeof(decimal) * 3)) = *(decimal*)(src + (sizeof(decimal) * 3));
                        *(decimal*)(dst + (sizeof(decimal) * 4)) = *(decimal*)(src + (sizeof(decimal) * 4));
                        *(decimal*)(dst + (sizeof(decimal) * 5)) = *(decimal*)(src + (sizeof(decimal) * 5));
                        *(decimal*)(dst + (sizeof(decimal) * 5) + remainingBytes) = *(decimal*)(src + (sizeof(decimal) * 5) + remainingBytes);
                        return;
                    }

                case 7:
                    {
                        *(decimal*)(dst) = *(decimal*)(src);
                        *(decimal*)(dst + sizeof(decimal)) = *(decimal*)(src + sizeof(decimal));
                        *(decimal*)(dst + (sizeof(decimal) * 2)) = *(decimal*)(src + (sizeof(decimal) * 2));
                        *(decimal*)(dst + (sizeof(decimal) * 3)) = *(decimal*)(src + (sizeof(decimal) * 3));
                        *(decimal*)(dst + (sizeof(decimal) * 4)) = *(decimal*)(src + (sizeof(decimal) * 4));
                        *(decimal*)(dst + (sizeof(decimal) * 5)) = *(decimal*)(src + (sizeof(decimal) * 5));
                        *(decimal*)(dst + (sizeof(decimal) * 6)) = *(decimal*)(src + (sizeof(decimal) * 6));
                        *(decimal*)(dst + (sizeof(decimal) * 6) + remainingBytes) = *(decimal*)(src + (sizeof(decimal) * 6) + remainingBytes);
                        return;
                    }

                case 8:
                    {
                        *(decimal*)(dst) = *(decimal*)(src);
                        *(decimal*)(dst + sizeof(decimal)) = *(decimal*)(src + sizeof(decimal));
                        *(decimal*)(dst + (sizeof(decimal) * 2)) = *(decimal*)(src + (sizeof(decimal) * 2));
                        *(decimal*)(dst + (sizeof(decimal) * 3)) = *(decimal*)(src + (sizeof(decimal) * 3));
                        *(decimal*)(dst + (sizeof(decimal) * 4)) = *(decimal*)(src + (sizeof(decimal) * 4));
                        *(decimal*)(dst + (sizeof(decimal) * 5)) = *(decimal*)(src + (sizeof(decimal) * 5));
                        *(decimal*)(dst + (sizeof(decimal) * 6)) = *(decimal*)(src + (sizeof(decimal) * 6));
                        *(decimal*)(dst + (sizeof(decimal) * 7)) = *(decimal*)(src + (sizeof(decimal) * 7));
                        return;
                    }
            }
        }
    }
}
