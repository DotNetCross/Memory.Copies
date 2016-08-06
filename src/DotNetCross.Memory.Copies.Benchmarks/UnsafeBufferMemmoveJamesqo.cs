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
    public class UnsafeBufferMemmoveJamesqo
    {
        internal unsafe static void Memmove(byte* dest, byte* src, nuint len)
        {
            // P/Invoke into the native version when the buffers are overlapping and the copy needs to be performed backwards
            // This check can produce false positives for lengths greater than Int32.MaxInt. It is fine because we want to use PInvoke path for the large lengths anyway.

            if ((nuint)dest - (nuint)src < len) goto PInvoke;

            // This is portable version of memcpy. It mirrors what the hand optimized assembly versions of memcpy typically do.
            //
            // Ideally, we would just use the cpblk IL instruction here. Unfortunately, cpblk IL instruction is not as efficient as
            // possible yet and so we have this implementation here for now.

            // Note: It's important that this switch handles lengths at least up to 22.
            // See notes below near the main loop for why.

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
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif
                    return;
                case 9:
#if BIT64
                    *(long*)dest = *(long*)src;
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif
                    *(dest + 8) = *(src + 8);
                    return;
                case 10:
#if BIT64
                    *(long*)dest = *(long*)src;
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif
                    *(short*)(dest + 8) = *(short*)(src + 8);
                    return;
                case 11:
#if BIT64
                    *(long*)dest = *(long*)src;
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif
                    *(short*)(dest + 8) = *(short*)(src + 8);
                    *(dest + 10) = *(src + 10);
                    return;
                case 12:
#if BIT64
                    *(long*)dest = *(long*)src;
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    return;
                case 13:
#if BIT64
                    *(long*)dest = *(long*)src;
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    *(dest + 12) = *(src + 12);
                    return;
                case 14:
#if BIT64
                    *(long*)dest = *(long*)src;
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    *(short*)(dest + 12) = *(short*)(src + 12);
                    return;
                case 15:
#if BIT64
                    *(long*)dest = *(long*)src;
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
#endif
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    *(short*)(dest + 12) = *(short*)(src + 12);
                    *(dest + 14) = *(src + 14);
                    return;
                case 16:
#if BIT64
                    *(long*)dest = *(long*)src;
                    *(long*)(dest + 8) = *(long*)(src + 8);
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
                *(int*)(dest + 8) = *(int*)(src + 8);
                *(int*)(dest + 12) = *(int*)(src + 12);
#endif
                    return;
                case 17:
#if BIT64
                    *(long*)dest = *(long*)src;
                    *(long*)(dest + 8) = *(long*)(src + 8);
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
                *(int*)(dest + 8) = *(int*)(src + 8);
                *(int*)(dest + 12) = *(int*)(src + 12);
#endif
                    *(dest + 16) = *(src + 16);
                    return;
                case 18:
#if BIT64
                    *(long*)dest = *(long*)src;
                    *(long*)(dest + 8) = *(long*)(src + 8);
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
                *(int*)(dest + 8) = *(int*)(src + 8);
                *(int*)(dest + 12) = *(int*)(src + 12);
#endif
                    *(short*)(dest + 16) = *(short*)(src + 16);
                    return;
                case 19:
#if BIT64
                    *(long*)dest = *(long*)src;
                    *(long*)(dest + 8) = *(long*)(src + 8);
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
                *(int*)(dest + 8) = *(int*)(src + 8);
                *(int*)(dest + 12) = *(int*)(src + 12);
#endif
                    *(short*)(dest + 16) = *(short*)(src + 16);
                    *(dest + 18) = *(src + 18);
                    return;
                case 20:
#if BIT64
                    *(long*)dest = *(long*)src;
                    *(long*)(dest + 8) = *(long*)(src + 8);
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
                *(int*)(dest + 8) = *(int*)(src + 8);
                *(int*)(dest + 12) = *(int*)(src + 12);
#endif
                    *(int*)(dest + 16) = *(int*)(src + 16);
                    return;
                case 21:
#if BIT64
                    *(long*)dest = *(long*)src;
                    *(long*)(dest + 8) = *(long*)(src + 8);
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
                *(int*)(dest + 8) = *(int*)(src + 8);
                *(int*)(dest + 12) = *(int*)(src + 12);
#endif
                    *(int*)(dest + 16) = *(int*)(src + 16);
                    *(dest + 20) = *(src + 20);
                    return;
                case 22:
#if BIT64
                    *(long*)dest = *(long*)src;
                    *(long*)(dest + 8) = *(long*)(src + 8);
#else
                *(int*)dest = *(int*)src;
                *(int*)(dest + 4) = *(int*)(src + 4);
                *(int*)(dest + 8) = *(int*)(src + 8);
                *(int*)(dest + 12) = *(int*)(src + 12);
#endif
                    *(int*)(dest + 16) = *(int*)(src + 16);
                    *(short*)(dest + 20) = *(short*)(src + 20);
                    return;
            }

            // P/Invoke into the native version for large lengths
            if (len >= 512) goto PInvoke;

            nuint i = 0; // byte offset at which we're copying

            if (((int)dest & 3) != 0)
            {
                if (((int)dest & 1) != 0)
                {
                    *(dest + i) = *(src + i);
                    i += 1;
                    if (((int)dest & 2) != 0)
                        goto IntAligned;
                }
                *(short*)(dest + i) = *(short*)(src + i);
                i += 2;
            }

            IntAligned:

#if BIT64
            // On 64-bit IntPtr.Size == 8, so we want to advance to the next 8-aligned address. If
            // (int)dest % 8 is 0, 5, 6, or 7, we will already have advanced by 0, 3, 2, or 1
            // bytes to the next aligned address (respectively), so do nothing. On the other hand,
            // if it is 1, 2, 3, or 4 we will want to copy-and-advance another 4 bytes until
            // we're aligned.
            // The thing 1, 2, 3, and 4 have in common that the others don't is that if you
            // subtract one from them, their 3rd lsb will not be set. Hence, the below check.

            if ((((int)dest - 1) & 4) == 0)
            {
                *(int*)(dest + i) = *(int*)(src + i);
                i += 4;
            }
#endif // BIT64

            nuint end = len - 16;
            len -= i; // lower 4 bits of len represent how many bytes are left *after* the unrolled loop

            // We know due to the above switch-case that this loop will always run 1 iteration; max
            // bytes we copy before checking is 23 (7 to align the pointers, 16 for 1 iteration) so
            // the switch handles lengths 0-22.
            //Contract.Assert(end >= 7 && i <= end);

            // This is separated out into a different variable, so the i + 16 addition can be
            // performed at the start of the pipeline and the loop condition does not have
            // a dependency on the writes.
            nuint counter;

            do
            {
                counter = i + 16;

                // This loop looks very costly since there appear to be a bunch of temporary values
                // being created with the adds, but the jit (for x86 anyways) will convert each of
                // these to use memory addressing operands.

                // So the only cost is a bit of code size, which is made up for by the fact that
                // we save on writes to dest/src.

#if BIT64
                *(long*)(dest + i) = *(long*)(src + i);
                *(long*)(dest + i + 8) = *(long*)(src + i + 8);
#else
                *(int*)(dest + i) = *(int*)(src + i);
                *(int*)(dest + i + 4) = *(int*)(src + i + 4);
                *(int*)(dest + i + 8) = *(int*)(src + i + 8);
                *(int*)(dest + i + 12) = *(int*)(src + i + 12);
#endif

                i = counter;

                // See notes above for why this wasn't used instead
                // i += 16;
            }
            while (counter <= end);

            if ((len & 8) != 0)
            {
#if BIT64
                *(long*)(dest + i) = *(long*)(src + i);
#else
                *(int*)(dest + i) = *(int*)(src + i);
                *(int*)(dest + i + 4) = *(int*)(src + i + 4);
#endif
                i += 8;
            }
            if ((len & 4) != 0)
            {
                *(int*)(dest + i) = *(int*)(src + i);
                i += 4;
            }
            if ((len & 2) != 0)
            {
                *(short*)(dest + i) = *(short*)(src + i);
                i += 2;
            }
            if ((len & 1) != 0)
            {
                *(dest + i) = *(src + i);
                // We're not using i after this, so not needed
                // i += 1;
            }

            return;

            PInvoke:
            //_Memmove(dest, src, len);
            memmove(dest, src, (int)len);

        }

        [DllImport("msvcrt.dll", SetLastError = false)]
        static unsafe extern IntPtr memmove(void* dest, void* src, int count);
    }
}
