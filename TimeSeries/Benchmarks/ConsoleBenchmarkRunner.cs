using System;
using System.Reflection;
using Benchmarks.Benchmarks;

namespace Benchmarks
{
    class ConsoleBenchmarkRunner
    {
        public void RunAll()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();

            var benchmarks = new BenchmarkFinder().GetBenchmarks(executingAssembly);

            foreach (var benchmark in benchmarks)
                RunSingleBenchmark(benchmark);

            Console.ReadKey();
        }

        private void RunSingleBenchmark(BenchmarksFixture benchmark)
        {
            benchmark.BenchmarkStarted += b => Console.WriteLine($"Running: {b.Name}");
            benchmark.IterationStarted += (b, i) => Console.Write(".");
            benchmark.BenchmarkFinished += (b, r) => Console.WriteLine($" done!\n{r.CreateReport()}\n");

            benchmark.Run();
        }
    }
}