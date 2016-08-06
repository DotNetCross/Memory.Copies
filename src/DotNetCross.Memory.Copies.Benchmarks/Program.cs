using System;
using System.Diagnostics;
using BenchmarkDotNet.Running;

namespace DotNetCross.Memory.Copies.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();

            var summary = BenchmarkRunner.Run<CopiesBenchmark>();

            sw.Stop();
            var elapsed = sw.ElapsedMilliseconds;
            var message = $"Benchmark time: {elapsed} ms";
            Trace.WriteLine(message);
            Console.WriteLine(message);
        }
    }
}
