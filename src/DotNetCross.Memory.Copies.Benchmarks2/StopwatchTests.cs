using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCross.Memory.Copies.Benchmarks2
{
    public static class StopwatchTests
    {
        private const int BufferSize = 1024 * 1024 * 1024;
        private static readonly byte[] _src = new byte[BufferSize];
        private static readonly byte[] _dst = new byte[BufferSize];
        private static int _copySize;
        private const long _max = 30000000;
        private const double TestTimeInMs = 1000.0;

        //Use a contant to change the test. Vars has huge impact on the performance
        private const int Cached = 0; //Pseudo Random Test
        private const int Randomizor = 0x13751; //Pseudo Random Test
        private const int Alignment = 1; //Alignment test
        private const int Sequence = -1; //read seq through array
        private const int TestMode = Cached; //Alignment test

        public static long GetMinIterations(int copyBytes)
        {
            var iterations = _max / (copyBytes == 0 ? 1 : copyBytes);
            var s0 = new Stopwatch();
            do
            {
                s0 = TestArrayCopy(copyBytes, iterations);
                if (s0.ElapsedMilliseconds <= 100)
                    iterations *= 10;
            } while (s0.ElapsedMilliseconds <= 100);
            iterations = (long)(iterations * (TestTimeInMs / s0.ElapsedMilliseconds));

            return iterations;
        }

        public static Stopwatch TestVectorizedIftree(int copyBytes, long interations)
        {
            var sw = Stopwatch.StartNew();
            var offset = 0;
            for (long i = 0; i < interations; i++)
            {
                offset += TestMode == -1 ? copyBytes : TestMode;
                if (offset + copyBytes >= BufferSize) offset &= 0x3fff;
                AndermanOptimized.Memmove(_src, offset, _dst, offset, copyBytes);
            }
            sw.Stop();
            return sw;
        }

        public static Stopwatch TestArrayCopy(int copyBytes, long interations)
        {
            var sw = Stopwatch.StartNew();
            var offset = 0;
            for (long i = 0; i < interations; i++)
            {
                offset += TestMode == -1 ? copyBytes : TestMode;
                if (offset + copyBytes >= BufferSize) offset &= 0x3fff;
                Array.Copy(_src, offset, _dst, offset, copyBytes);
            }
            sw.Stop();
            return sw;
        }

        public static Stopwatch TestUnsafeBufferMemmoveJamesqo2(int copyBytes, long interations)
        {
            var sw = Stopwatch.StartNew();
            var offset = 0;
            for (long i = 0; i < interations; i++)
            {
                offset += TestMode == -1 ? copyBytes : TestMode;
                if (offset + copyBytes >= BufferSize) offset &= 0x3fff;
                UnsafeBufferMemmoveJamesqo2.Memmove(_src, offset, _dst, offset, copyBytes);
            }
            sw.Stop();
            return sw;
        }

        public static Stopwatch TestMsMemmove(int copyBytes, long interations)
        {
            var sw = Stopwatch.StartNew();
            var offset = 0;
            for (long i = 0; i < interations; i++)
            {
                offset += TestMode == -1 ? copyBytes : TestMode;
                if (offset + copyBytes >= BufferSize) offset &= 0x3fff;
                MsvcrtMemove.Memmove(_src, offset, _dst, offset, copyBytes);
            }
            sw.Stop();
            return sw;
        }

        private static Stopwatch TestUnsafeCpblk(int copyBytes, long interations)
        {
            var sw = Stopwatch.StartNew();
            var offset = 0;
            for (long i = 0; i < interations; i++)
            {
                offset += TestMode == -1 ? copyBytes : TestMode;
                if (offset + copyBytes >= BufferSize) offset &= 0x3fff;
                UnsafeCpblk.Copy(_src, offset, _dst, offset, copyBytes);
            }
            sw.Stop();
            return sw;
        }

    }
}
