using BenchmarkDotNet.Running;

namespace DotNetCross.Memory.Copies.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<CopiesBenchmark>();
        }
    }
}
