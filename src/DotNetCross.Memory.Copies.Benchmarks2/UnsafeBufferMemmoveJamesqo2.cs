#define BIT64
#define AMD64
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
#if BIT64
using nuint = System.UInt64;
#else // BIT64
using nuint = System.UInt32;
#endif // BIT64
namespace test
{
    public class UnsafeBufferMemmoveJamesqo2
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

        [System.Security.SecurityCritical]
        internal static unsafe void Memmove(byte* dest, byte* src, nuint len)
        {
            // P/Invoke into the native version when the buffers are overlapping and the copy needs to be performed backwards
            // This check can produce false positives for lengths greater than int.MaxValue. It is fine because we want to use the P/Invoke path for the large lengths anyway.

            //if ((nuint)dest - (nuint)src < len) goto PInvoke;

            // This is portable version of memcpy. It mirrors what the hand optimized assembly versions of memcpy typically do.
            //
            // Ideally, we would just use the cpblk IL instruction here. Unfortunately, cpblk IL instruction is not as efficient as
            // possible yet and so we have this implementation here for now.

            // Note: It's important that this switch handles lengths at least up to 15 for AMD64.
            // We assume below len is at least 16 and make one 128-bit write without checking.

            // The switch will be very fast since it can be implemented using a jump
            // table in assembly. See http://stackoverflow.com/a/449297/4077294 for more info.

            switch (len)
            {
                case 0:
                    return;
                case 1:
                    *dest = *src;
                    return;
                case 2:
                    *(short*)dest = *(short*)src;
                    return;
                case 3:
                    *(short*)dest = *(short*)src;
                    *(dest + 2) = *(src + 2);
                    return;
                case 4:
                    *(int*)dest = *(int*)src;
                    return;
                case 5:
                    *(int*)dest = *(int*)src;
                    *(dest + 4) = *(src + 4);
                    return;
                case 6:
                    *(int*)dest = *(int*)src;
                    *(short*)(dest + 4) = *(short*)(src + 4);
                    return;
                case 7:
                    *(int*)dest = *(int*)src;
                    *(short*)(dest + 4) = *(short*)(src + 4);
                    *(dest + 6) = *(src + 6);
                    return;
                case 8:
#if BIT64
                    *(long*)dest = *(long*)src;
#else // BIT64
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                    return;
                case 9:
#if BIT64
                    *(long*)dest = *(long*)src;
#else // BIT64
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                    *(dest + 8) = *(src + 8);
                    return;
                case 10:
#if BIT64
                    *(long*)dest = *(long*)src;
#else // BIT64
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                    *(short*)(dest + 8) = *(short*)(src + 8);
                    return;
                case 11:
#if BIT64
                    *(long*)dest = *(long*)src;
#else // BIT64
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                    *(short*)(dest + 8) = *(short*)(src + 8);
                    *(dest + 10) = *(src + 10);
                    return;
                case 12:
#if BIT64
                    *(long*)dest = *(long*)src;
#else // BIT64
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    return;
                case 13:
#if BIT64
                    *(long*)dest = *(long*)src;
#else // BIT64
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    *(dest + 12) = *(src + 12);
                    return;
                case 14:
#if BIT64
                    *(long*)dest = *(long*)src;
#else // BIT64
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    *(short*)(dest + 12) = *(short*)(src + 12);
                    return;
                case 15:
#if BIT64
                    *(long*)dest = *(long*)src;
#else // BIT64
                    *(int*)dest = *(int*)src;
                    *(int*)(dest + 4) = *(int*)(src + 4);
#endif // BIT64
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    *(short*)(dest + 12) = *(short*)(src + 12);
                    *(dest + 14) = *(src + 14);
                    return;
            }

            // P/Invoke into the native version for large lengths.
            // Currently the threshold at which the native version is faster seems to be around 8192
            // on amd64 Windows, but this is subject to change if this implementation can be made faster.
            //if (len >= 8192) goto PInvoke;

            // So far SIMD is only enabled for AMD64, so on that plaform we want
            // to 16-byte align while on others (including arm64) we'll want to word-align
#if AMD64
            nuint alignment = 16u;
#else // AMD64
            nuint alignment = (nuint)sizeof(nuint);
#endif // AMD64

            // (nuint)dest % alignment calculates how far we are from the previous aligned address
            // Note that it's *very* important alignment is unsigned.
            // (int)dest % (int)alignment for example will give different results if the lhs is negative.

            // If dest is aligned this will be 0.
            nuint i = (nuint)dest % alignment;

            // We know from the above switch-case that len is at least 16, so here
            // we subtract i from 16. This represents the furthest aligned address
            // we know it's okay to write upto.
            // To make it clearer, (dest + i) after this is equivalent to
            // [previous aligned address] + 16.
            i = 16u - i;

#if AMD64
    // SIMD is enabled for AMD64, so take advantage of that and use movdqu
            *(Buffer16*)dest = *(Buffer16*)src;
#elif ARM64
    // ARM64 has 64-bit words but no SIMD yet, so make 2 word writes
    // First one isn't aligned, second one is (remember from earlier notes dest + i is 8-aligned)
            *(long*)dest = *(long*)src;
            *(long*)(dest + i - 8) = *(long*)(src + i - 8);
#else // AMD64, ARM64
            // i386 and ARM: 32-bit words, no SIMD (yet)
            // make 1 unaligned word write, then 3 4-byte aligned ones
            *(int*)dest = *(int*)src;
            *(int*)(dest + i - 12) = *(int*)(src + i - 12);
            *(int*)(dest + i - 8) = *(int*)(src + i - 8);
            *(int*)(dest + i - 4) = *(int*)(src + i - 4);
#endif // AMD64, ARM64

            // i now represents the number of bytes we've copied so far.
            //Contract.Assert(i <= len && i > 0 && i <= 16);
            //Contract.Assert((nuint)(dest + i) % alignment == 0);

            // chunk: bytes processed per iteration in unrolled loop
            // Note: Not directly related to sizeof(nuint), e.g. sizeof(nuint) * 8 is not a valid substitution.
            nuint chunk = sizeof(nuint) == 4 ? 32 : 64;

            // mask: represents how many bytes are left after alignment
            // Since we copy the bytes in chunks of 2, mask will also have the lower few
            // bits of mask (mask & (chunk - 1), but we don't explicitly calculate that)
            // will represent how many bytes are left *after* the unrolled loop.
            nuint mask = len - i;

            // Protect ourselves from unsigned overflow
            if (len < chunk)
                goto LoopCleanup;

            // end: point after which we stop the unrolled loop
            // This is the end of the buffer, minus the space
            // required for 1 iteration of the loop.
            nuint end = len - chunk;

            // This can return false in the first iteration if the process of
            // aligning the pointer for writes has not left enough space
            // for this loop to run, so unfortunately this can't be a do-while loop.
            while (i <= end)
            {
                // Some versions of this loop looks very costly since there appear
                // to be a bunch of temporary values being created with the adds,
                // but the jit (for x86 anyways) will convert each of these to
                // use memory addressing operands.

                // So the only cost is a bit of code size, which is made up for by the fact that
                // we save on writes to dest/src.

#if AMD64
    // Write 64 bytes at a time, taking advantage of xmm register on AMD64
    // This will be translated to 4 movdqus (maybe movdqas in the future, dotnet/coreclr#2725)
                *(Buffer64*)(dest + i) = *(Buffer64*)(src + i);
#elif ARM64
    // ARM64: Also unroll by 64 bytes, this time using longs since we don't
    // take advantage of SIMD for that plaform yet.
                *(long*)(dest + i) = *(long*)(src + i);
                *(long*)(dest + i + 8) = *(long*)(src + i + 8);
                *(long*)(dest + i + 16) = *(long*)(src + i + 16);
                *(long*)(dest + i + 24) = *(long*)(src + i + 24);
                *(long*)(dest + i + 32) = *(long*)(src + i + 32);
                *(long*)(dest + i + 40) = *(long*)(src + i + 40);
                *(long*)(dest + i + 48) = *(long*)(src + i + 48);
                *(long*)(dest + i + 56) = *(long*)(src + i + 56);
#else // AMD64, ARM64
                // i386/ARM32:
                // Write 32 bytes at a time, via 8 32-bit word writes
                *(int*)(dest + i) = *(int*)(src + i);
                *(int*)(dest + i + 4) = *(int*)(src + i + 4);
                *(int*)(dest + i + 8) = *(int*)(src + i + 8);
                *(int*)(dest + i + 12) = *(int*)(src + i + 12);
                *(int*)(dest + i + 16) = *(int*)(src + i + 16);
                *(int*)(dest + i + 20) = *(int*)(src + i + 20);
                *(int*)(dest + i + 24) = *(int*)(src + i + 24);
                *(int*)(dest + i + 28) = *(int*)(src + i + 28);
#endif // AMD64, ARM64

                i += chunk;
            }

            LoopCleanup:
            // If we've reached this point, there are at most chunk - 1 bytes left

#if BIT64
    // mask & 63 represents how many bytes there are left.
    // if the mask & 32 bit is set that means this number
    // will be >= 32. (same principle applies for other
    // powers of 2 below)
            if ((mask & 32) != 0)
            {
#if AMD64
                *(Buffer32*)(dest + i) = *(Buffer32*)(src + i);
#else // AMD64
                *(long*)(dest + i) = *(long*)(src + i);
                *(long*)(dest + i + 8) = *(long*)(src + i + 8);
                *(long*)(dest + i + 16) = *(long*)(src + i + 16);
                *(long*)(dest + i + 24) = *(long*)(src + i + 24);
#endif // AMD64

                i += 32;
            }
#endif // BIT64

            // Now there can be at most 31 bytes left

            if ((mask & 16) != 0)
            {
#if AMD64
                *(Buffer16*)(dest + i) = *(Buffer16*)(src + i);
#elif ARM64
                *(long*)(dest + i) = *(long*)(src + i);
                *(long*)(dest + i + 8) = *(long*)(src + i + 8);
#else // AMD64, ARM64
                *(int*)(dest + i) = *(int*)(src + i);
                *(int*)(dest + i + 4) = *(int*)(src + i + 4);
                *(int*)(dest + i + 8) = *(int*)(src + i + 8);
                *(int*)(dest + i + 12) = *(int*)(src + i + 12);
#endif // AMD64, ARM64

                i += 16;
            }

            // Now there can be at most 15 bytes left
            // For AMD64 we just want to make 1 (potentially) unaligned xmm write and quit.
            // For other platforms we have another switch-case for 0..15.
            // Again, this is implemented with a jump table so it's very fast.

#if AMD64
            i = len - 16;
            *(Buffer16*)(dest + i) = *(Buffer16*)(src + i);
#else // AMD64

            switch (mask & 15)
            {
                case 0:
                    // No-op: We already finished copying all the bytes.
                    return;
                case 1:
                    *(dest + i) = *(src + i);
                    return;
                case 2:
                    *(short*)(dest + i) = *(short*)(src + i);
                    return;
                case 3:
                    *(short*)(dest + i) = *(short*)(src + i);
                    *(dest + i + 2) = *(src + i + 2);
                    return;
                case 4:
                    *(int*)(dest + i) = *(int*)(src + i);
                    return;
                case 5:
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(dest + i + 4) = *(src + i + 4);
                    return;
                case 6:
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(short*)(dest + i + 4) = *(short*)(src + i + 4);
                    return;
                case 7:
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(short*)(dest + i + 4) = *(short*)(src + i + 4);
                    *(dest + i + 6) = *(src + i + 6);
                    return;
                case 8:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    return;
                case 9:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    *(dest + i + 8) = *(src + i + 8);
                    return;
                case 10:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    *(short*)(dest + i + 8) = *(short*)(src + i + 8);
                    return;
                case 11:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    *(short*)(dest + i + 8) = *(short*)(src + i + 8);
                    *(dest + i + 10) = *(src + i + 10);
                    return;
                case 12:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    *(int*)(dest + i + 8) = *(int*)(src + i + 8);
                    return;
                case 13:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    *(int*)(dest + i + 8) = *(int*)(src + i + 8);
                    *(dest + i + 12) = *(src + i + 12);
                    return;
                case 14:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    *(int*)(dest + i + 8) = *(int*)(src + i + 8);
                    *(short*)(dest + i + 12) = *(short*)(src + i + 12);
                    return;
                case 15:
#if BIT64
                    *(long*)(dest + i) = *(long*)(src + i);
#else // BIT64
                    *(int*)(dest + i) = *(int*)(src + i);
                    *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif // BIT64
                    *(int*)(dest + i + 8) = *(int*)(src + i + 8);
                    *(short*)(dest + i + 12) = *(short*)(src + i + 12);
                    *(dest + i + 14) = *(src + i + 14);
                    return;
            }

#endif // AMD64

            return;

            //PInvoke:
            //_Memmove(dest, src, len);

        }

        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct Buffer64
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 32)]
        private struct Buffer32
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        private struct Buffer16
        {
        }
    }
}