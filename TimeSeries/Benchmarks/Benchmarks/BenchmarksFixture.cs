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

        protected abstract IEnumerable<IBenchmark> GetBenchmarks();

        public event Action<IBenchmark> BenchmarkSetup;
        public event Action<IBenchmark> BenchmarkStarted;
        public event Action<IBenchmark, IBenchmarkingResult> BenchmarkFinished;
        public event Action<IBenchmark> BenchmarkTeardown;

        public event Action OnClassSetup;
        public event Action OnClassTearDown;

        public void Run()
        {
            OnClassSetup?.Invoke();
            ClassSetUp();

            GetBenchmarks().ToList().ForEach(RunSingleBenchmark);

            OnClassTearDown?.Invoke();
            ClassTearDown();
        }

        private void RunSingleBenchmark(IBenchmark benchmark)
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