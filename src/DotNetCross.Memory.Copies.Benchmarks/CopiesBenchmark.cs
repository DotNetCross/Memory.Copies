using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace DotNetCross.Memory.Copies.Benchmarks
{
    [Config(typeof(Config))]
    public class CopiesBenchmark
    {
        class Config : ManualConfig
        {
            public Config()
            {
                Add(Job.AllJits.Select(j => j.WithLaunchCount(1).WithWarmupCount(1).WithTargetCount(5)).ToArray());
                //Add(Job.RyuJitX64.WithLaunchCount(1).WithWarmupCount(1).WithTargetCount(5));
                Add(StatisticColumn.AllStatistics);
            }
        }

        //[Params(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 512, 512 + 31, 1024)]
        [Params(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32,
            33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64,
            65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96,
            128, 256, 508, 509, 510, 511, 512, 513, 512 + 31, 512 + 32, 512 + 35, 512 + 64,
            1024, 2048, 4096, 2 * 4096, 8512, 128 * 1024)]
        public int BytesCopied = 0;
        //[Params(0)]
        public int Index = 0;

        const int BufferSize = 16 * 1024 * 1024;
        private readonly static byte[] bufferFrom = new byte[BufferSize];
        private readonly static byte[] bufferTo = new byte[BufferSize];

        [Benchmark(Baseline = true)]
        public void ArrayCopy()
        {
            Array.Copy(bufferFrom, Index, bufferTo, Index, BytesCopied);
        }

        [Benchmark]
        public void BufferBlockCopy()
        {
            Buffer.BlockCopy(bufferFrom, Index, bufferTo, Index, BytesCopied);
        }

        [Benchmark]
        public void IllyriadVectorizedCopy()
        {
            Illyriad.VectorizedCopy(bufferFrom, Index, bufferTo, Index, BytesCopied);
        }

        [Benchmark]
        public void AndermanVectorizedCopy()
        {
            Anderman.VectorizedCopy(bufferFrom, Index, bufferTo, Index, BytesCopied);
        }

        [Benchmark]
        public unsafe void UnsafeIllyriadVectorizedCopy()
        {
            UnsafeIllyriad.VectorizedCopy(bufferFrom, Index, bufferTo, Index, BytesCopied);
        }

        [Benchmark]
        public unsafe void UnsafeAndermanVectorizedCopy()
        {
            UnsafeAnderman.VectorizedCopy(bufferFrom, Index, bufferTo, Index, BytesCopied);
        }

        [Benchmark]
        public unsafe void UnsafeCopyBlock()
        {
            // No checks
            fixed (byte* src = bufferFrom)
            fixed (byte* dst = bufferFrom)
            {
                Unsafe.CopyBlock(dst + Index, src + Index, (uint)BytesCopied);
            }
        }

        [Benchmark]
        public unsafe void Buffer_MemmoveOriginal()
        {
            UnsafeBufferMemmoveOriginal.Memmove(bufferFrom, Index, bufferTo, Index, BytesCopied);
        }

        [Benchmark]
        public unsafe void Buffer_MemmoveJamesqo()
        {
            UnsafeBufferMemmoveJamesqo.Memmove(bufferFrom, Index, bufferTo, Index, BytesCopied);
        }

        [Benchmark]
        public unsafe void Buffer_MemmoveTannerGooding()
        {
            UnsafeBufferMemmoveTannerGooding.Memmove(bufferFrom, Index, bufferTo, Index, BytesCopied);
        }
    }
}
