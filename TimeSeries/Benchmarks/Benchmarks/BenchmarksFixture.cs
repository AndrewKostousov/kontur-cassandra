using System;
using System.Collections.Generic;
using System.Linq;

namespace Benchmarks.Benchmarks
{
    class BenchmarksFixture
    {
        [From(typeof(BenchmarkClassSetUpAttribute))]
        public Action ClassSetup { get; set; }

        [From(typeof(BenchmarkClassTearDownAttribute))]
        public Action ClassTeardown { get; set; }

        [From(typeof(BenchmarkSetUpAttribute))]
        public Action Setup { get; set; }

        [From(typeof(BenchmarkTearDownAttribute))]
        public Action Teardown { get; set; }

        [From(typeof(BenchmarkMethodAttribute))]
        public IEnumerable<Benchmark> Benchmarks { get; } = new List<Benchmark>();

        [From(typeof(BenchmarkResultAttribute))]
        public List<Func<IBenchmarkingResult>> AdditionalResults { get; } = new List<Func<IBenchmarkingResult>>();

        public event Action<Benchmark> BenchmarkStarted;
        public event Action<Benchmark, int> IterationStarted;
        public event Action<Benchmark, int> IterationFinished;
        public event Action<Benchmark, IBenchmarkingResult> BenchmarkFinished;

        public List<IBenchmarkingResult> Run()
        {
            ClassSetup?.Invoke();

            var result = Benchmarks
                .Select(ConnectGlobalHandlers)
                .Select(RunSingleBenchmark)
                .ToList();

            ClassTeardown?.Invoke();

            return result;
        }

        private IBenchmarkingResult RunSingleBenchmark(Benchmark benchmark)
        {
            Setup?.Invoke();
            BenchmarkStarted?.Invoke(benchmark);

            var result = benchmark.Run();

            foreach (var getResult in AdditionalResults)
                result.AddResult(getResult());

            BenchmarkFinished?.Invoke(benchmark, result);
            Teardown?.Invoke();
            return result;
        }

        private Benchmark ConnectGlobalHandlers(Benchmark benchmark)
        {
            benchmark.IterationStarted += i => IterationStarted?.Invoke(benchmark, i);
            benchmark.IterationFinished += i => IterationFinished?.Invoke(benchmark, i);

            return benchmark;
        }
    }
}