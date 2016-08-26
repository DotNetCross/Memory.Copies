using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCross.Memory.Copies.Benchmarks2
{
    public static class Tests
    {
        private const int BufferSize = 1024 * 1024 * 1024;
        private static readonly byte[] _src = new byte[BufferSize];
        private static readonly byte[] _dst = new byte[BufferSize];

        const int MinIterations = 50;
        public static ulong TestDuration = 1000000;

        public static unsafe int GetOffsetDst()
        {
            fixed (byte* pSrcOrigin = &_dst[0])
            {
                return 0x10 - (int)((ulong)pSrcOrigin & 0xF);
            }
        }

        public static unsafe int GetOffsetSrc()
        {
            fixed (byte* pSrcOrigin = &_src[0])
            {
                return 0x10 - (int)((ulong)pSrcOrigin & 0xF);
            }
        }

        public static void Warmup()
        {
            for (var i = 1; i < 256; i++)
            {
                _src[i] = (byte)i;
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

        public static double TestOverhead()
        {
            var mincycles = ulong.MaxValue;
            var startTest = Rdtsc.TimestampP();
            var testCycles = 0UL;

            int dummy = 10;
            do
            {
                ulong cycles = 0;
                for (var j = 1; j < 1000; j++)
                {
                    var start = Rdtsc.TimestampP();
                    for (var h = 1; h < MinIterations; h++)
                    {
                        dummy = h;
                    }
                    var end = Rdtsc.TimestampP();
                    cycles += end - start;
                }

                if (cycles < mincycles) mincycles = cycles;

                testCycles = Rdtsc.TimestampP() - startTest;
            } while (testCycles < TestDuration && testCycles > 0);
            return dummy > 0 ? (mincycles / 1000.0) / (double)MinIterations : 0;
        }


        public static double TestAnderman(int srcOffset, int dstOffset, int size)
        {
            var mincycles = ulong.MaxValue;
            var startTest = Rdtsc.TimestampP();
            var testCycles = 0UL;

            do
            {
                var start = Rdtsc.TimestampP();
                for (var j = 1; j < MinIterations; j++)
                {
                    AndermanOptimized.Memmove(_src, srcOffset, _dst, dstOffset, size);
                }
                var end = Rdtsc.TimestampP();
                var cycles = end - start;
                if (cycles <= mincycles)
                {
                    mincycles = cycles;
                }
                testCycles = Rdtsc.TimestampP() - startTest;
            } while (testCycles < TestDuration && testCycles > 0);
            return (mincycles / (double)MinIterations);
        }

        public static double TestJames(int srcOffset, int dstOffset, int size)
        {
            var mincycles = ulong.MaxValue;
            var startTest = Rdtsc.TimestampP();
            var testCycles = 0UL;

            do
            {
                var start = Rdtsc.TimestampP();
                for (var j = 1; j < MinIterations; j++)
                {
                    UnsafeBufferMemmoveJamesqo2.Memmove(_src, srcOffset, _dst, dstOffset, size);
                }
                var end = Rdtsc.TimestampP();
                var cycles = end - start;
                if (cycles <= mincycles)
                {
                    mincycles = cycles;
                }
                testCycles = Rdtsc.TimestampP() - startTest;
            } while (testCycles < TestDuration && testCycles > 0);
            return (mincycles / (double)MinIterations);
        }

        public static double TestMsvcrtMemmove(int srcOffset, int dstOffset, int size)
        {
            var mincycles = ulong.MaxValue;
            var startTest = Rdtsc.TimestampP();
            var testCycles = 0UL;

            do
            {
                var start = Rdtsc.TimestampP();
                for (var j = 1; j < MinIterations; j++)
                {
                    MsvcrtMemove.Memmove(_src, srcOffset, _dst, dstOffset, size);
                }
                var end = Rdtsc.TimestampP();
                var cycles = end - start;
                if (cycles <= mincycles)
                {
                    mincycles = cycles;
                }
                testCycles = Rdtsc.TimestampP() - startTest;
            } while (testCycles < TestDuration && testCycles > 0);
            return (mincycles / (double)MinIterations);
        }

        public static double TestArray(int srcOffset, int dstOffset, int size)
        {
            var mincycles = ulong.MaxValue;
            var startTest = Rdtsc.TimestampP();
            var testCycles = 0UL;

            do
            {
                var start = Rdtsc.TimestampP();
                for (var j = 1; j < MinIterations; j++)
                {
                    Array.Copy(_src, srcOffset, _dst, dstOffset, size);
                }
                var end = Rdtsc.TimestampP();
                var cycles = end - start;
                if (cycles <= mincycles)
                {
                    mincycles = cycles;
                }
                testCycles = Rdtsc.TimestampP() - startTest;
            } while (testCycles < TestDuration && testCycles > 0);
            return (mincycles / (double)MinIterations);
        }

        public static double TestMovSb(int srcOffset, int dstOffset, int size)
        {
            var mincycles = ulong.MaxValue;
            var startTest = Rdtsc.TimestampP();
            var testCycles = 0UL;

            do
            {
                var start = Rdtsc.TimestampP();
                for (var j = 1; j < MinIterations; j++)
                {
                    AndermanMovsb.Memmove(_src, srcOffset, _dst, dstOffset, size);
                }
                var end = Rdtsc.TimestampP();
                var cycles = end - start;
                if (cycles <= mincycles)
                {
                    mincycles = cycles;
                }
                testCycles = Rdtsc.TimestampP() - startTest;
            } while (testCycles < TestDuration && testCycles > 0);
            return (mincycles / (double)MinIterations);
        }

        public static double TestDelegate(Func<int, int, int, ulong> copyAction, int srcOffset, int dstOffset, int size)
        {
            var mincycles = ulong.MaxValue;
            var startTest = Rdtsc.TimestampP();
            var testCycles = 0UL;

            do
            {
                var cycles = copyAction(srcOffset, dstOffset, size);
                if (cycles <= mincycles)
                {
                    mincycles = cycles;
                }
                testCycles = Rdtsc.TimestampP() - startTest;
            } while (testCycles < TestDuration && testCycles > 0);

            return (mincycles / (double)MinIterations);
        }

        public  static ulong TestAndermanDelegate(int srcOffset, int dstOffset, int size)
        {
            var start = Rdtsc.TimestampP();
            for (var j = 1; j < MinIterations; j++)
            {
                AndermanOptimized.Memmove(_src, srcOffset, _dst, dstOffset, size);
            }
            var end = Rdtsc.TimestampP();
            return end - start;
        }
    }
}
