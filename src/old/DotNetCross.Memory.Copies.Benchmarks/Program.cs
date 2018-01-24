using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BenchmarkDotNet.Running;

namespace DotNetCross.Memory.Copies.Benchmarks
{
    static class Program
    {
        static readonly Action<string> Log = t => { Trace.WriteLine(t); Console.WriteLine(t); };

        static void Main(string[] args)
        {
            var choices = new CommandChoice[]
            {
                new CommandChoice("Run Copies Benchmark (managed array)", RunCopiesBenchmark),
                new CommandChoice("Run Unsafe No Checks Copies Benchmark (native array)", RunUnsafeNoChecksCopiesBenchmark),
                new CommandChoice("Output new BytesToCopy list", OutputBytesToCopyList),
            };

            WriteChoices(choices);

            var action = SelectCommand(choices);

            action();
        }

        public struct CommandChoice
        {
            public readonly string Description;
            public readonly Action Action;
            public CommandChoice(string description, Action action)
            {
                if (description == null) { throw new ArgumentNullException(nameof(description)); }
                if (action == null) { throw new ArgumentNullException(nameof(action)); }
                Description = description;
                Action = action;
            }
        }

        private static Action SelectCommand(CommandChoice[] choices)
        {
            while (true)
            {
                var keyPressed = Console.ReadKey(true);
                int digit;
                if (keyPressed.KeyChar.TryParseInt(out digit) &&
                    digit > 0 &&
                    digit <= choices.Length)
                {
                    --digit;
                    var chosen = choices[digit];
                    Log($"You selected '{keyPressed.KeyChar}':'{chosen.Description}'");
                    return chosen.Action;
                }
                else
                {
                    Log($"You pressed '{keyPressed.KeyChar}' which is not supported");
                }
            }
        }

        private static void WriteChoices(CommandChoice[] choices)
        {
            Log("Select command to execute:");
            for (int i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];

                Log($"{i + 1}: {choice.Description}");
            }
        }

        private static void RunCopiesBenchmark()
        {
            var sw = new Stopwatch();
            sw.Start();

            var summary = BenchmarkRunner.Run<CopiesBenchmark>();

            sw.Stop();
            var elapsed = sw.ElapsedMilliseconds;
            var message = $"Benchmark time: {elapsed} ms";
            Log(message);
        }

        private static void RunUnsafeNoChecksCopiesBenchmark()
        {
            var sw = new Stopwatch();
            sw.Start();

            var summary = BenchmarkRunner.Run<UnsafeNoChecksCopiesBenchmark>();

            sw.Stop();
            var elapsed = sw.ElapsedMilliseconds;
            var message = $"Benchmark time: {elapsed} ms";
            Log(message);
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
            var verytiny = Enumerable.Range(0, 32).ToArray();

            var tiny = Enumerable.Range(0, 32).Select(i => 32 + i * 2).ToArray();

            var small = Enumerable.Range(3, 28).Select(i => i * 32).ToArray();
            var smallExtra = small.Select(i => i + 7).Concat(small.Select(i => i + 17)).Concat(small.Select(i => i + 26)).ToArray();

            var medium = Enumerable.Range(4, 29).Select(i => i * 256).ToArray();
            var mediumExtra = medium.Select(i => i + 62).Concat(medium.Select(i => i + 131)).Concat(medium.Select(i => i + 199)).ToArray();

            var all = verytiny.Concat(tiny).Concat(small).Concat(smallExtra).Concat(medium).Concat(mediumExtra).Distinct().ToArray();
            Array.Sort(all); // Sort so they are ordered
            return all;
        }

        public static bool TryParseInt(this char value, out int digit)
        {
            digit = (int)(value - '0');
            if (digit >= 0 && digit <= 9)
            { return true; }
            return false;
        }
    }
}
