using System;

namespace DotNetCross.Memory.Copies.Benchmarks2
{
    public static class Tests
    {
        private const int BufferSize = 1024*1024*1024;
        private static readonly byte[] _src = new byte[BufferSize];
        private static readonly byte[] _dst = new byte[BufferSize];

        private const int Cached = 0; //Pseudo Random Test
        private const int Randomizor = +0x3313751; //Pseudo Random Test
        private const int Alignment = 1; //Alignment test
        private const int Sequence = -1; //read seq through array
        private const int TestMode = Cached; //Alignment test

        private const int MinIterations = 50;
        public static ulong TestDuration = 1000000;

        public static unsafe int GetOffsetDst()
        {
            fixed (byte* pSrcOrigin = &_dst[0])
            {
                return 0x10 - (int) ((ulong) pSrcOrigin & 0xF);
            }
        }

        public static unsafe int GetOffsetSrc()
        {
            fixed (byte* pSrcOrigin = &_src[0])
            {
                return 0x10 - (int) ((ulong) pSrcOrigin & 0xF);
            }
        }

        public static void Warmup()
        {
            for (var i = 1; i < 256; i++)
            {
                _src[i] = (byte) i;
                _dst[i] = 0;
            }
            for (var size = 0; size < 32768; size++)
            {
                AndermanOptimized.Memmove(_src, 0, _dst, 0, size);
                for (var j = 0; j < size + 0; j++)
                {
                    if (_src[j] != _dst[j])
                        throw new Exception($"i={size}, j={j} _src[j]({_src[j]}) != _dst[j]({_dst[j]})");
                    _dst[j] = 0;
                }
            }
        }

        public static double TestOverhead(int offset, int size)
        {
            var mincycles = ulong.MaxValue;
            var startTest = Rdtsc.TimestampP();
            var testCycles = 0UL;

            do
            {
                ulong cycles = 0;
                for (var j = 1; j < 1000; j++)
                {
                    var start = Rdtsc.TimestampP();
                    for (var h = 1; h < MinIterations; h++)
                    {
                        offset += TestMode == -1 ? size : TestMode;
                        if (offset + size >= BufferSize) offset &= 0xFFfff;
                    }
                    var end = Rdtsc.TimestampP();
                    cycles += end - start;
                }

                if (cycles < mincycles) mincycles = cycles;

                testCycles = Rdtsc.TimestampP() - startTest;
            } while (testCycles < TestDuration && testCycles > 0);
            return offset >= 0 ? mincycles/1000.0/MinIterations : 0;
        }

        public static double TestAnderman(int offset, int size)
        {
            var mincycles = ulong.MaxValue;
            var startTest = Rdtsc.TimestampP();
            var testCycles = 0UL;

            do
            {
                var start = Rdtsc.TimestampP();
                for (var j = 1; j < MinIterations; j++)
                {
                    offset += TestMode == -1 ? size : TestMode;
                    if (offset + size >= BufferSize) offset &= 0xFFfff;
                    AndermanOptimized.Memmove(_src, offset, _dst, offset, size);
                }
                var end = Rdtsc.TimestampP();
                var cycles = end - start;
                if (cycles <= mincycles)
                {
                    mincycles = cycles;
                }
                testCycles = Rdtsc.TimestampP() - startTest;
            } while (testCycles < TestDuration && testCycles > 0);
            return mincycles/(double) MinIterations;
        }

        public static double TestJames(int offset,  int size)
        {
            var mincycles = ulong.MaxValue;
            var startTest = Rdtsc.TimestampP();
            var testCycles = 0UL;

            do
            {
                var start = Rdtsc.TimestampP();
                for (var j = 1; j < MinIterations; j++)
                {
                    offset += TestMode == -1 ? size : TestMode;
                    if (offset + size >= BufferSize) offset &= 0xFFfff;
                    UnsafeBufferMemmoveJamesqo2.Memmove(_src, offset, _dst, offset, size);
                }
                var end = Rdtsc.TimestampP();
                var cycles = end - start;
                if (cycles <= mincycles)
                {
                    mincycles = cycles;
                }
                testCycles = Rdtsc.TimestampP() - startTest;
            } while (testCycles < TestDuration && testCycles > 0);
            return mincycles/(double) MinIterations;
        }

        public static double TestMsvcrtMemmove(int offset, int size)
        {
            var mincycles = ulong.MaxValue;
            var startTest = Rdtsc.TimestampP();
            var testCycles = 0UL;

            do
            {
                var start = Rdtsc.TimestampP();
                for (var j = 1; j < MinIterations; j++)
                {
                    offset += TestMode == -1 ? size : TestMode;
                    if (offset + size >= BufferSize) offset &= 0xFFfff;
                    MsvcrtMemove.Memmove(_src, offset, _dst, offset, size);
                }
                var end = Rdtsc.TimestampP();
                var cycles = end - start;
                if (cycles <= mincycles)
                {
                    mincycles = cycles;
                }
                testCycles = Rdtsc.TimestampP() - startTest;
            } while (testCycles < TestDuration && testCycles > 0);
            return mincycles/(double) MinIterations;
        }

        public static double TestArray(int offset, int size)
        {
            var mincycles = ulong.MaxValue;
            var startTest = Rdtsc.TimestampP();
            var testCycles = 0UL;

            do
            {
                var start = Rdtsc.TimestampP();
                for (var j = 1; j < MinIterations; j++)
                {
                    offset += TestMode == -1 ? size : TestMode;
                    if (offset + size >= BufferSize) offset &= 0xFFfff;
                    Array.Copy(_src, offset, _dst, offset, size);
                }
                var end = Rdtsc.TimestampP();
                var cycles = end - start;
                if (cycles <= mincycles)
                {
                    mincycles = cycles;
                }
                testCycles = Rdtsc.TimestampP() - startTest;
            } while (testCycles < TestDuration && testCycles > 0);
            return mincycles/(double) MinIterations;
        }

        public static double TestMovSb(int offset, int size)
        {
            var mincycles = ulong.MaxValue;
            var startTest = Rdtsc.TimestampP();
            var testCycles = 0UL;

            do
            {
                var start = Rdtsc.TimestampP();
                for (var j = 1; j < MinIterations; j++)
                {
                    offset += TestMode == -1 ? size : TestMode;
                    if (offset + size >= BufferSize) offset &= 0xFFfff;
                    AndermanMovsb.Memmove(_src, offset, _dst, offset, size);
                }
                var end = Rdtsc.TimestampP();
                var cycles = end - start;
                if (cycles <= mincycles)
                {
                    mincycles = cycles;
                }
                testCycles = Rdtsc.TimestampP() - startTest;
            } while (testCycles < TestDuration && testCycles > 0);
            return mincycles/(double) MinIterations;
        }

        public static double TestDelegate(Func<int, int, int, ulong> copyAction, int offset, int size)
        {
            var mincycles = ulong.MaxValue;
            var startTest = Rdtsc.TimestampP();
            var testCycles = 0UL;

            do
            {
                var cycles = copyAction(offset, offset, size);
                if (cycles <= mincycles)
                {
                    mincycles = cycles;
                }
                testCycles = Rdtsc.TimestampP() - startTest;
            } while (testCycles < TestDuration && testCycles > 0);

            return mincycles/(double) MinIterations;
        }

        public static ulong TestAndermanDelegate(int offset, int size)
        {
            var start = Rdtsc.TimestampP();
            for (var j = 1; j < MinIterations; j++)
            {
                offset += TestMode == -1 ? size : TestMode;
                if (offset + size >= BufferSize) offset &= 0xFFfff;
                AndermanOptimized.Memmove(_src, offset, _dst, offset, size);
            }
            var end = Rdtsc.TimestampP();
            return end - start;
        }
    }
}