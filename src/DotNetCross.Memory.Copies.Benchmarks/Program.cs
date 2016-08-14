using System;
using System.Diagnostics;
using System.Linq;
using BenchmarkDotNet.Running;

namespace DotNetCross.Memory.Copies.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //OutputBytesToCopyList();
            //return;

            var sw = new Stopwatch();
            sw.Start();

            var summary = BenchmarkRunner.Run<CopiesBenchmark>();

            sw.Stop();
            var elapsed = sw.ElapsedMilliseconds;
            var message = $"Benchmark time: {elapsed} ms";
            Trace.WriteLine(message);
            Console.WriteLine(message);
        }

        static void OutputBytesToCopyList()
        {
            var bytesToCopyList = GenerateBytesToCopyList();
            var paramsText = string.Join(",", bytesToCopyList);
            Trace.WriteLine(paramsText);
            Console.WriteLine(paramsText);
        }

        static int[] GenerateBytesToCopyList()
        {
            var tiny = Enumerable.Range(0, 96).ToArray();
            
            var small = Enumerable.Range(3, 28).Select(i => i * 32).ToArray();
            var smallExtra = small.Select(i => i + 7).Concat(small.Select(i => i + 17)).Concat(small.Select(i => i + 26)).ToArray();

            var medium = Enumerable.Range(4, 32).Select(i => i * 256).ToArray();
            var mediumExtra = medium.Select(i => i + 62).Concat(medium.Select(i => i + 131)).Concat(medium.Select(i => i + 199)).ToArray();

            var all = tiny.Concat(small).Concat(smallExtra).Concat(medium).Concat(mediumExtra).Distinct().ToArray();
            Array.Sort(all); // Sort so they are ordered
            return all;
        }
    }
}
