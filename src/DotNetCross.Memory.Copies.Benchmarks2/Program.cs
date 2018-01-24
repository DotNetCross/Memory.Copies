using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace DotNetCross.Memory.Copies.Benchmarks2
{
    public class Program
    {
        private const ulong TestDuration = 100;
        //private static extern bool QueryThreadCycleTime(IntPtr hThread, out ulong cycles);
        //private static readonly IntPtr PseudoHandle = (IntPtr)(-2);
        public static double LoopOverhead;
        public static ulong CyclesPerSecond;
        public static double NsPerCycle;

        public static void Main(string[] args)
        {
            Console.WriteLine($"Warmup...");
            //Tests.Warmup();
            CyclesPerSecond = GetCyclesPerSeond();
            NsPerCycle = 1000*1000*1000.0/CyclesPerSecond;
            Tests.TestDuration = TestDuration*CyclesPerSecond/1000;
            LoopOverhead = Tests.TestOverhead(1000,1000);

            Console.WriteLine($"offset src:      {Tests.GetOffsetSrc():X04}");
            Console.WriteLine($"offset dst:      {Tests.GetOffsetDst():X04} ");
            Console.WriteLine($"CyclesPerSecond: {CyclesPerSecond,5:0} ");
            Console.WriteLine($"nsPerCycle:      {NsPerCycle,5:0.0000} ");
            Console.WriteLine($"loopOverhead:    {LoopOverhead,5:0.0000} Cycles");
            Console.WriteLine($"                 {LoopOverhead*NsPerCycle,5:0.0000} ns ");
            Console.WriteLine($"Starting...");


            do
            {
                var googleChart = new GoogleChart
                {
                    cols = new[]
                    {
                        new Col {label = "X", type = "number"},
                        new Col {label = "ArrayCopy", type = "number"},
                        new Col {label = "MsvcrtMemmove", type = "number"},
                        new Col {label = "AndermanMovsb", type = "number"},
                        new Col {label = "Anderman", type = "number"}
                    }
                };
                var sizes = new[]
                {
                    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 54, 56, 58, 60, 62, 64, 66, 68, 70, 72, 74, 76, 78, 80, 82, 84, 86, 88, 90, 92, 94, 96, 103, 113, 122, 128, 135,
                    145,
                    154, 160, 167, 177, 186, 192, 199, 209, 218, 224, 231, 241, 250, 256, 263, 273, 282, 288, 295, 305, 314, 320, 327, 337, 346, 352, 359, 369, 378, 384, 391, 401, 410, 416, 423, 433, 442, 448, 455, 465, 474, 480, 487, 497, 506, 512, 519, 529, 538, 544, 551, 561, 570, 576, 583,
                    593, 602, 608, 615, 625, 634, 640, 647, 657, 666, 672, 679, 689, 698, 704, 711, 721, 730, 736, 743, 753, 762, 768, 775, 785, 794, 800, 807, 817, 826, 832, 839, 849, 858, 864, 871, 881, 890, 896, 903, 913, 922, 928, 935, 945, 954, 960, 967, 977, 986, 1024, 1086, 1155, 1223,
                    1280, 1342, 1411, 1479, 1536, 1598, 1667, 1735, 1792, 1854, 1923, 1991, 2048, 2110, 2179, 2247, 2304, 2366, 2435, 2503, 2560, 2622, 2691, 2759, 2816, 2878, 2947, 3015, 3072, 3134, 3203, 3271, 3328, 3390, 3459, 3527, 3584, 3646, 3715, 3783, 3840, 3902, 3971, 4039, 4096, 4158,
                    4227, 4295, 4352, 4414, 4483, 4551, 4608, 4670, 4739, 4807, 4864, 4926, 4995, 5063, 5120, 5182, 5251, 5319, 5376, 5438, 5507, 5575, 5632, 5694, 5763, 5831, 5888, 5950, 6019, 6087, 6144, 6206, 6275, 6343, 6400, 6462, 6531, 6599, 6656, 6718, 6787, 6855, 6912, 6974, 7043, 7111,
                    7168, 7230, 7299, 7367, 7424, 7486, 7555, 7623, 7680, 7742, 7811, 7879, 7936, 7998, 8067, 8135, 8192, 8254, 8323, 8391, 16384, 32768, 65536, 131072, 262144,262144+256,262144+512, 524288, 1048576, 2*1048576, 4*1048576, 8*1048576
                };
                var selectedSizes = sizes.Where(x => x >= 0 && x < 10).ToArray();
                googleChart.rows = new Row[selectedSizes.Count()];
                var index = 0;
                foreach (var size in selectedSizes)
                {
                    var cycles = Tests.TestArray(0,  size) - LoopOverhead;
                    var cycles0 = Tests.TestArray(0,  size) - LoopOverhead;
                    var cycles2 = Tests.TestMovSb(0, size) - LoopOverhead;
                    var cycles3 = Tests.TestAnderman(0, size) - LoopOverhead;
                    googleChart.rows[index++] = new Row
                    {
                        c = new[]
                        {
                            new C {v = size},
                            new C {v = cycles0},
                            new C {v = cycles2},
                            new C {v = cycles3}
                        }
                    };

                    //double cycles3 = TestCode(TestAnderman);
                    Console.WriteLine($"{size:0} {cycles,8:0.00}  {cycles2,8:0.00} {cycles3,8:0.00}  ");
                }
                Console.WriteLine("ready");
                File.WriteAllText(@"chart.json", "chartData=" + JsonConvert.SerializeObject(googleChart));

                Tests.Warmup();
            } while (false);
        }

        private static ulong GetCyclesPerSeond()
        {
            var sw = Stopwatch.StartNew();
            var startms = Rdtsc.TimestampP();
            do
            {
            } while (sw.ElapsedMilliseconds < 1000);
            var endms = Rdtsc.TimestampP();

            return endms - startms;
        }
    }
}