#define BIT64
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
#if BIT64
using nuint = System.UInt64;
#else // BIT64
using nuint = System.UInt32;
#endif // BIT64

namespace DotNetCross.Memory.Copies.Benchmarks
{
    // tannergooding2 optimized https://github.com/dotnet/coreclr/pull/6638 and https://github.com/dotnet/coreclr/pull/6627
    public class UnsafeBufferMemmoveTannerGooding2
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

                Memmove(pDst, pSrc, (nuint)count);
            }
        }

        const int sizeof_UInt128 = sizeof(ulong) * 2;

        [StructLayout(LayoutKind.Sequential, Pack = 16, Size = sizeof_UInt128)]
        struct UInt128
        {
            public ulong loPart;
            public ulong hiPart;
        }

        const int sizeof_UInt256 = sizeof_UInt128 * 2;

        [StructLayout(LayoutKind.Sequential, Pack = 16, Size = sizeof_UInt256)]
        struct UInt256
        {
            public UInt128 loPart;
            public UInt128 hiPart;
        }

#if BIT64
        const int sizeof_UInt512 = sizeof_UInt256 * 2;

        [StructLayout(LayoutKind.Sequential, Pack = 16, Size = sizeof_UInt512)]
        struct UInt512
        {
            public UInt256 loPart;
            public UInt256 hiPart;
        }
#endif

        public unsafe static void Memmove(byte* dst, byte* src, nuint len)
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
                        *(UInt128*)(dst) = *(UInt128*)(src);
                        return;
                    }
            }

            if (len <= 32)
            {
                // We can do this in two writes. Note that one or both of these writes may be misaligned

                *(UInt128*)(dst) = *(UInt128*)(src);
                *(UInt128*)(dst + len - sizeof_UInt128) = *(UInt128*)(src + len - sizeof_UInt128);

                return;
            }

            var misalignment = ((nuint)(dst) % sizeof_UInt128);

            if (misalignment != 0)
            {
                *(UInt128*)(dst) = *(UInt128*)(src);
                *(UInt128*)(dst + misalignment) = *(UInt128*)(src + misalignment);

                var initialOffset = (sizeof_UInt128 + misalignment);

                len -= initialOffset;
                src += initialOffset;
                dst += initialOffset;
            }

#if BIT64
            const nuint blockSize = sizeof_UInt512;
#else
            const nuint blockSize = sizeof_UInt256;
#endif

            if (len > blockSize)
            {
                var iterations = (len / blockSize);

                for (var iteration = 0ul; iteration < iterations; iteration++)
                {
#if BIT64
                    *(UInt512*)(dst) = *(UInt512*)(src);
#else
                    *(UInt256*)(dst) = *(UInt256*)(src);
#endif

                    src += blockSize;
                    dst += blockSize;
                }

                len -= (iterations * blockSize);
            }

            if (len == 0)
            {
                return;
            }

            var remainingBlocks = (len / sizeof_UInt128);
            var remainingBytes = (len - (remainingBlocks * sizeof_UInt128));

            switch (remainingBlocks)
            {
                case 0:
                    {
                        *(UInt128*)(dst - sizeof_UInt128 + remainingBytes) = *(UInt128*)(src - sizeof_UInt128 + remainingBytes);
                        return;
                    }

                case 1:
                    {
                        *(UInt128*)(dst) = *(UInt128*)(src);
                        *(UInt128*)(dst + remainingBytes) = *(UInt128*)(src + remainingBytes);
                        return;
                    }

#if BIT64
                case 2:
                {
                    *(UInt256*)(dst) = *(UInt256*)(src);
                    *(UInt128*)(dst + sizeof_UInt128 + remainingBytes) = *(UInt128*)(src + sizeof_UInt128 + remainingBytes);
                    return;
                }

                case 3:
                {
                    *(UInt256*)(dst) = *(UInt256*)(src);
                    *(UInt128*)(dst + sizeof_UInt256) = *(UInt128*)(src + sizeof_UInt256);
                    *(UInt128*)(dst + sizeof_UInt256 + remainingBytes) = *(UInt128*)(src + sizeof_UInt256 + remainingBytes);
                    return;
                }

                case 4:
                {
                    *(UInt512*)(dst) = *(UInt512*)(src);
                    return;
                }
#else
                case 2:
                    {
                        *(UInt256*)(dst) = *(UInt256*)(src);
                        return;
                    }
#endif
            }
        }
    }
}
