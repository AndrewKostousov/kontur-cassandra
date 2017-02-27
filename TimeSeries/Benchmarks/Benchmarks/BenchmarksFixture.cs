using System;
using System.Collections.Generic;
using System.Linq;

namespace Benchmarks.Benchmarks
{
    public abstract class BenchmarksFixture
    {
        public abstract string Name { get; }

        protected virtual void ClassSetUp() { }
        protected virtual void ClassTearDown() { }

        protected abstract IEnumerable<Benchmark> GetBenchmarks();

        public event Action<Benchmark> BenchmarkSetup;
        public event Action<Benchmark> BenchmarkStarted;
        public event Action<Benchmark, IBenchmarkingResult> BenchmarkFinished;
        public event Action<Benchmark> BenchmarkTeardown;

        public void Run()
        {
            ClassSetUp();

            GetBenchmarks().ToList().ForEach(RunSingleBenchmark);

            ClassTearDown();
        }

        private void RunSingleBenchmark(Benchmark benchmark)
        {
            BenchmarkSetup?.Invoke(benchmark);

            benchmark.SetUp();

            BenchmarkStarted?.Invoke(benchmark);

            var result = benchmark.Run();

            BenchmarkFinished?.Invoke(benchmark, result);

            benchmark.TearDown();

            BenchmarkTeardown?.Invoke(benchmark);
        }
    }
}