using System;
using System.Collections.Generic;
using Benchmarks.Benchmarks;

namespace Benchmarks.Runners
{
    class ConsoleBenchmarkRunner : IBenchmarkRunner
    {
        readonly string separator = "\n".PadLeft(100, '=') + "\n";

        public void RunAll(IEnumerable<BenchmarksFixture> benchmarks)
        {
            foreach (var fixture in benchmarks)
                RunSingleBenchmark(fixture);

            Console.WriteLine(separator + "All benchmarks done! Press any key to exit.");
            Console.ReadKey();
        }

        private void RunSingleBenchmark(BenchmarksFixture fixture)
        {
            Console.WriteLine(separator + $"Preparing fixture: {fixture.Name}\n");

            fixture.BenchmarkSetupStarted += b => Console.Write(separator + $"Setting up: {b.Name} ... ");
            fixture.IterationStarted += (b, i) => Console.Write($"Done!\nRunning benchmark: {b.Name} ... ");
            fixture.BenchmarkFinished += (b, r) => Console.WriteLine($"Done!\n\n{r.CreateReport()}");

            fixture.Run();
        }
    }
}