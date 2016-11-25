using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Benchmarks.Results;

namespace Benchmarks.Benchmarks
{
    public class Benchmark
    {
        public int IterationsCount { get; }
        public string Name { get; }

        private Action OnRun;
        public Func<IBenchmarkingResult> AdditionalResult { get; }

        public event Action<int> IterationStarted;
        public event Action<int> IterationFinished;

        public Benchmark(string name, int iterationsCount, Action onRun, Func<IBenchmarkingResult> additionalResult=null)
        {
            OnRun = onRun;
            IterationsCount = iterationsCount;
            Name = name;
            AdditionalResult = additionalResult;
        }

        public IEnumerable<IBenchmarkingResult> Run()
        {
            for (var i = 0; i < IterationsCount; ++i)
            {
                IterationStarted?.Invoke(i);
                yield return RunIteration();
                IterationFinished?.Invoke(i);
            }
        }

        private IBenchmarkingResult RunIteration()
        {
            return new BenchmarkingResult(DoRun(), AdditionalResult?.Invoke());
        }

        private TimeSpan DoRun()
        {
            var sw = Stopwatch.StartNew();
            OnRun();
            return sw.Elapsed;
        }
    }
}
