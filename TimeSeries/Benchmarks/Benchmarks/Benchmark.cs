using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Benchmarks.Benchmarks
{
    public abstract class Benchmark
    {
        protected abstract int IterationsCount { get; }
        public abstract string Name { get; }

        protected abstract void OnPrepare();
        protected abstract void OnRun();

        public event Action Started;
        public event Action<int> IterationStarted;
        public event Action<int> IterationFinished;
        public event Action<TimeSpan> Finished;

        public TimeSpan Run()
        {
            Started?.Invoke();

            OnPrepare();
            

            var millisecondsPerIteration = (double) sw.ElapsedMilliseconds/IterationsCount;
            var iterationTime = TimeSpan.FromMilliseconds(millisecondsPerIteration);

            Finished?.Invoke(iterationTime);
            return iterationTime;
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

        private TimeSpan Mean()
        {

        }
    }
}
