using System;
using System.Runtime.InteropServices;

namespace DotNetCross.Memory.Copies.Benchmarks
{
    public unsafe delegate void Memmove(void* pDst, void* pSrc, int count);

    public unsafe class MemmoveTests
    {
        const int MaxByteCount = 1024;
        const int MaxOffset = 16;
        const int MaxSize = MaxByteCount + MaxOffset;
        static readonly byte* Source = (byte*)Marshal.AllocHGlobal(MaxSize).ToPointer();
        static readonly byte* Destination = (byte*)Marshal.AllocHGlobal(MaxSize).ToPointer();

        public static void TestMemmove(Memmove memmove)
        {
            var source = Source;
            var destination = Destination;
            // Only need to randomize source once
            RandomizeMemory(source, MaxSize);

            for (var byteCount = 0; byteCount < 1024; byteCount++)
            {
                for (var sourceOffset = 0; sourceOffset < 16; sourceOffset++)
                {
                    for (var destinationOffset = 0; destinationOffset < 16; destinationOffset++)
                    {
                        ZeroMemory(destination, byteCount + destinationOffset);

                        var offsetDst = destination + destinationOffset;
                        var offsetSrc = source + sourceOffset;

                        memmove(offsetDst, offsetSrc , byteCount);

                        ValidateMemory(offsetSrc, offsetDst, byteCount);
                    }
                }
            }
        }

        static void RandomizeMemory(byte* destination, int byteCount)
        {
            var rng = new Random();

            for (var index = 0; index < byteCount; index++)
            {
                *(destination + index) = (byte)(rng.Next(byte.MinValue, byte.MaxValue));
            }
        }

        static void ZeroMemory(byte* destination, int byteCount)
        {
            for (var index = 0; index < byteCount; index++)
            {
                *(destination + index) = 0;
            }
        }

        static void ValidateMemory(byte* source, byte* destination, int byteCount)
        {
            for (var index = 0; index < byteCount; index++)
            {
                var areEqual = (*(destination + index) == *(source + index));

                if (!areEqual)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
        }
    }
}
