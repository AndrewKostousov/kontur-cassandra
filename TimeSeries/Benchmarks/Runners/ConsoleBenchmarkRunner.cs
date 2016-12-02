using System;
using System.Reflection;
using Benchmarks.Benchmarks;
using Benchmarks.Reflection;

namespace Benchmarks
{
    class ConsoleBenchmarkRunner
    {
        readonly string separator = "\n".PadLeft(100, '=') + "\n";

        public void RunAll(Assembly assembly)
        {
            Console.WriteLine($"Retrieving benchmarks from <{assembly.FullName}>\n");

            var benchmarkFixtures = new BenchmarkFinder().GetBenchmarks(assembly);

            foreach (var fixture in benchmarkFixtures)
                RunSingleBenchmark(fixture);

            Console.WriteLine(separator + "All benchmarks done! Press any key to exit.");
            Console.ReadKey();
        }

        private void RunSingleBenchmark(BenchmarksFixture fixture)
        {
            Console.WriteLine(separator + $"Preparing fixture: {fixture.Name}\n");

            fixture.BenchmarkStarted += b => Console.Write(separator + $"Running benchmark: {b.Name} ... ");
            fixture.BenchmarkFinished += (b, r) => Console.WriteLine($"Done!\n\n{r.CreateReport()}\n");

            fixture.Run();
        }
    }
}