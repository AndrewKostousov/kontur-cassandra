using System;
using System.Reflection;
using Benchmarks.Benchmarks;

namespace Benchmarks
{
    class BenchmarkRunner
    {
        public void RunAll()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();

            var benchmarks = new Extractor().Extract<Benchmark>(executingAssembly);

            foreach (var benchmark in benchmarks)
                RunSingleBenchmark(benchmark);

            Console.ReadKey();
        }

        private void RunSingleBenchmark(Benchmark benchmark)
        {
            benchmark.Started += () => Console.WriteLine($"Running {benchmark.Name}...");
            benchmark.IterationStarted += i => Console.Write(".");
            benchmark.Finished += t => Console.WriteLine($" done!\nmean time: {t}\n");

            benchmark.Run();
        }
    }
}