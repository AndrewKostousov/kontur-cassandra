using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Commons;

namespace Benchmarks.Benchmarks
{
    public class Benchmark
    {
        protected int IterationsCount { get; }
        public string Name { get; }

        private Action OnRun;

        public Benchmark(string name, int iterationsCount, Action onRun)
        {
            OnRun = onRun;
            IterationsCount = iterationsCount;
            Name = name;
        }
        
        public event Action<int> IterationStarted;
        public event Action<int> IterationFinished;

        public IBenchmarkingResult Run()
        {
            var timeSpent = TimeSpan.FromTicks((long)MeasureTimes().Average(t => t.Ticks));
            return new BenchmarkingResult(timeSpent);
        }

        private IEnumerable<TimeSpan> MeasureTimes()
        {
            var sw = new Stopwatch();

            for (var i = 0; i < IterationsCount; ++i)
            {
                IterationStarted?.Invoke(i);

                sw.Restart();
                OnRun();
                sw.Stop();

                yield return sw.Elapsed;

                IterationFinished?.Invoke(i);
            }
        }
    }
}
