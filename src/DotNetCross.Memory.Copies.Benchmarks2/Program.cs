using System;
using System.Diagnostics;

namespace test
{
    public class Program
    {
        private const long _max = 30000000;
        private const int BufferSize = 1024*1024*1024;
        private static readonly byte[] _src = new byte[BufferSize];
        private static readonly byte[] _dst = new byte[BufferSize];

        public static void Main(string[] args)
        {
            Console.WriteLine("Initalizing setup random");

            Console.WriteLine("Test en compile functions");
            InitArray(Array.Copy);
            InitArray(MsvcrtMemove.MsvcrtMemmove);
            InitArray(Anderman2.VectorizedCopyAnderman);
            InitArray(UnsafeBufferMemmoveJamesqo2.Memmove);


            Console.WriteLine($"bytes\titerations\tarray\tmsmemmove\tanderman\tUnsafeBufferMemmoveJamesqo2");

            foreach (var copyBytes in new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 54, 56, 58, 60, 62, 64, 66, 68, 70, 72, 74, 76, 78, 80, 82, 84, 86, 88, 90, 92, 94, 96})
                //foreach (var copyBytes in new[] { 128,256,512,1024,2048,4096,8192,16384,32768,65536})
                //foreach (var copyBytes in new[] { 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16})
                //foreach (var copyBytes in new[] {103, 113, 122, 128, 135, 145, 154, 160, 167, 177, 186, 192, 199, 209, 218, 224, 231, 241, 250, 256, 263, 273, 282, 288, 295, 305, 314, 320, 327, 337, 346, 352, 359, 369, 378, 384, 391, 401, 410, 416, 423, 433, 442, 448, 455, 465, 474, 480, 487, 497, 506, 512, 519, 529, 538, 544, 551, 561, 570, 576, 583, 593, 602, 608, 615, 625, 634, 640, 647, 657, 666, 672, 679, 689, 698, 704, 711, 721, 730, 736, 743, 753, 762, 768, 775, 785, 794, 800, 807, 817, 826, 832, 839, 849, 858, 864, 871, 881, 890, 896, 903, 913, 922, 928, 935, 945, 954, 960, 967, 977, 986, 1024, 1086, 1155, 1223, 1280, 1342, 1411, 1479, 1536, 1598, 1667, 1735, 1792, 1854, 1923, 1991, 2048, 2110, 2179, 2247, 2304, 2366, 2435, 2503, 2560, 2622, 2691, 2759, 2816, 2878, 2947, 3015, 3072, 3134, 3203, 3271, 3328, 3390, 3459, 3527, 3584, 3646, 3715, 3783, 3840, 3902, 3971, 4039, 4096, 4158, 4227, 4295, 4352, 4414, 4483, 4551, 4608, 4670, 4739, 4807, 4864, 4926, 4995, 5063, 5120, 5182, 5251, 5319, 5376, 5438, 5507, 5575, 5632, 5694, 5763, 5831, 5888, 5950, 6019, 6087, 6144, 6206, 6275, 6343, 6400, 6462, 6531, 6599, 6656, 6718, 6787, 6855, 6912, 6974, 7043, 7111, 7168, 7230, 7299, 7367, 7424, 7486, 7555, 7623, 7680, 7742, 7811, 7879, 7936, 7998, 8067, 8135, 8192, 8254, 8323, 8391})
            {
                Test(copyBytes);
            }
            Console.WriteLine("ready");
            Console.ReadKey();
        }

        private static void InitArray(Action<byte[], int, byte[], int, int> copyAction)
        {
            for (var i = 1; i < 256; i++)
            {
                _src[i] = (byte) i;
            }
            for (var i = 0; i < 256; i++)
            {
                copyAction(_src, 0, _dst, 0, i);
                for (var j = 0; i < 256; i++)
                {
                    if (_src[j] != _dst[j])
                        throw new Exception($"i={i} _src[j]({_src[j]})!= _dst[j]({_dst[j]})");
                }
            }
        }

        private static void Test(int copyBytes)
        {
            var iterations = (int) _max/(copyBytes == 0 ? 1 : copyBytes);

            var s0 = TestArrayCopy(copyBytes, iterations);
            iterations = (int) (iterations*(2000.0/s0.ElapsedMilliseconds));

            var s4 = TestUnsafeBufferMemmoveJamesqo2(copyBytes, iterations);
            var s1 = TestArrayCopy(copyBytes, iterations);
            var s2 = TestMsMemmove(copyBytes, iterations);
            var s3 = TestVectorizedIftree(copyBytes, iterations);

            double baseline = s1.Elapsed.Ticks;
            Console.Write($"{copyBytes}");
            Console.Write($"\t{iterations}");
            Console.Write($"\t{(double) s1.Elapsed.Ticks/baseline:  0.00}");
            Console.Write($"\t{(double) s2.Elapsed.Ticks/baseline:  0.00}");
            Console.Write($"\t{(double) s3.Elapsed.Ticks/baseline:  0.00}");
            Console.Write($"\t{(double) s4.Elapsed.Ticks/baseline:  0.00}");
            Console.Write($"\t{s1.Elapsed.TotalMilliseconds*1000000/iterations:  0.00}");
            Console.Write($"\t{s2.Elapsed.TotalMilliseconds*1000000/iterations:  0.00}");
            Console.Write($"\t{s3.Elapsed.TotalMilliseconds*1000000/iterations:  0.00}");
            Console.Write($"\t{s4.Elapsed.TotalMilliseconds*1000000/iterations:  0.00}");
            Console.WriteLine();
        }

        private static Stopwatch TestVectorizedIftree(int copyBytes, int interations)
        {
            var sw = Stopwatch.StartNew();
            var offset = 0;
            for (long i = 0; i < interations; i++)
            {
                offset += 0x4731;
                if (offset + copyBytes >= BufferSize) offset &= 0x3fff;
                Anderman2.VectorizedCopyAnderman(_src, offset, _dst, offset, copyBytes);
            }
            sw.Stop();
            return sw;
        }

        private static Stopwatch TestArrayCopy(int copyBytes, int interations)
        {
            var sw = Stopwatch.StartNew();
            var offset = 0;
            for (long i = 0; i < interations; i++)
            {
                offset += 0x4731;
                if (offset + copyBytes >= BufferSize) offset &= 0x3fff;
                Array.Copy(_src, offset, _dst, offset, copyBytes);
            }
            sw.Stop();
            return sw;
        }

        private static Stopwatch TestUnsafeBufferMemmoveJamesqo2(int copyBytes, int interations)
        {
            var sw = Stopwatch.StartNew();
            var offset = 0;
            for (long i = 0; i < interations; i++)
            {
                offset += 0x4731;
                if (offset + copyBytes >= BufferSize) offset &= 0x3fff;
                UnsafeBufferMemmoveJamesqo2.Memmove(_src, offset, _dst, offset, copyBytes);
            }
            sw.Stop();
            return sw;
        }

        private static Stopwatch TestMsMemmove(int copyBytes, int interations)
        {
            var sw = Stopwatch.StartNew();
            var offset = 0;
            for (long i = 0; i < interations; i++)
            {
                offset += 0x4731;
                if (offset + copyBytes >= BufferSize) offset &= 0x3fff;
                MsvcrtMemove.MsvcrtMemmove(_src, offset, _dst, offset, copyBytes);
            }
            sw.Stop();
            return sw;
        }
    }
}