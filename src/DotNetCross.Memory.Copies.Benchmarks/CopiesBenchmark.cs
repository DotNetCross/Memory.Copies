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
                //Add(Job.AllJits.Select(j => j.WithLaunchCount(1).WithWarmupCount(1).WithTargetCount(5)).ToArray());
                Add(Job.RyuJitX64.WithLaunchCount(1).WithWarmupCount(1).WithTargetCount(5));
                Add(StatisticColumn.AllStatistics);
            }
        }

        // NOTE: Use generator in Program.cs
        [Params(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 54, 56, 58, 60, 62, 64, 66, 68, 70, 72, 74, 76, 78, 80, 82, 84, 86, 88, 90, 92, 94, 96, 103, 113, 122, 128, 135, 145, 154, 160, 167, 177, 186, 192, 199, 209, 218, 224, 231, 241, 250, 256, 263, 273, 282, 288, 295, 305, 314, 320, 327, 337, 346, 352, 359, 369, 378, 384, 391, 401, 410, 416, 423, 433, 442, 448, 455, 465, 474, 480, 487, 497, 506, 512, 519, 529, 538, 544, 551, 561, 570, 576, 583, 593, 602, 608, 615, 625, 634, 640, 647, 657, 666, 672, 679, 689, 698, 704, 711, 721, 730, 736, 743, 753, 762, 768, 775, 785, 794, 800, 807, 817, 826, 832, 839, 849, 858, 864, 871, 881, 890, 896, 903, 913, 922, 928, 935, 945, 954, 960, 967, 977, 986, 1024, 1086, 1155, 1223, 1280, 1342, 1411, 1479, 1536, 1598, 1667, 1735, 1792, 1854, 1923, 1991, 2048, 2110, 2179, 2247, 2304, 2366, 2435, 2503, 2560, 2622, 2691, 2759, 2816, 2878, 2947, 3015, 3072, 3134, 3203, 3271, 3328, 3390, 3459, 3527, 3584, 3646, 3715, 3783, 3840, 3902, 3971, 4039, 4096, 4158, 4227, 4295, 4352, 4414, 4483, 4551, 4608, 4670, 4739, 4807, 4864, 4926, 4995, 5063, 5120, 5182, 5251, 5319, 5376, 5438, 5507, 5575, 5632, 5694, 5763, 5831, 5888, 5950, 6019, 6087, 6144, 6206, 6275, 6343, 6400, 6462, 6531, 6599, 6656, 6718, 6787, 6855, 6912, 6974, 7043, 7111, 7168, 7230, 7299, 7367, 7424, 7486, 7555, 7623, 7680, 7742, 7811, 7879, 7936, 7998, 8067, 8135, 8192, 8254, 8323, 8391)]
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

        //[Benchmark]
        public void SimpleLoop()
        {
            // No checks since it is not unsafe
            for (int i = 0; i < BytesCopied; i++)
            {
                bufferTo[i + Index] = bufferFrom[i + Index];
            }
        }

        [Benchmark]
        public void BufferBlockCopy()
        {
            Buffer.BlockCopy(bufferFrom, Index, bufferTo, Index, BytesCopied);
        }

        //[Benchmark]
        public void BufferMemoryCopy()
        {
            UnsafeBufferMemoryCopy.Copy(bufferFrom, Index, bufferTo, Index, BytesCopied);
        }

        [Benchmark]
        public void MsvcrtMemmove()
        {
            Msvcrt.Memmove(bufferFrom, Index, bufferTo, Index, BytesCopied);
        }

        //[Benchmark]
        public void MsvcrtMemmoveOverhead()
        {
            Msvcrt.Memmove(bufferFrom, Index, bufferTo, Index, 0);
        }

        [Benchmark]
        public unsafe void UnsafeCopyBlock()
        {
            UnsafeCpblk.Copy(bufferFrom, Index, bufferTo, Index, BytesCopied);
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
        public void AndermanVectorizedSwitch()
        {
            UnsafeAnderman2.VectorizedCopy2(bufferFrom, Index, bufferTo, Index, BytesCopied);
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

        [Benchmark]
        public unsafe void Buffer_MemmoveJamesqo2()
        {
            UnsafeBufferMemmoveJamesqo2.Memmove(bufferFrom, Index, bufferTo, Index, BytesCopied);
        }

        [Benchmark]
        public unsafe void Buffer_MemmoveTannerGooding2()
        {
            UnsafeBufferMemmoveTannerGooding2.Memmove(bufferFrom, Index, bufferTo, Index, BytesCopied);
        }
    }
}
