using System;
using System.Reflection;
using Benchmarks.Benchmarks;
using Benchmarks.Reflection;

namespace Benchmarks
{
    class ConsoleBenchmarkRunner
    {
        public void RunAll(Assembly assembly)
        {
            Console.WriteLine($"Retrieving benchmarks from <{assembly.FullName}>\n");

            var benchmarkFixtures = new BenchmarkFinder().GetBenchmarks(assembly);

            foreach (var fixture in benchmarkFixtures)
                RunSingleBenchmark(fixture);

            Console.WriteLine("All benchmarks done! Press any key to exit.");
            Console.ReadKey();
        }

        private void RunSingleBenchmark(BenchmarksFixture fixture)
        {
            Console.WriteLine($"Running fixture: {fixture.Name}\n");

            fixture.BenchmarkStarted += b => Console.WriteLine($"Running benchmark: {b.Name}");
            fixture.IterationStarted += (b, i) => Console.Write(".");
            fixture.BenchmarkFinished += (b, r) => Console.WriteLine($" done!\n{r.CreateReport()}\n");

            fixture.Run();
        }
    }
}