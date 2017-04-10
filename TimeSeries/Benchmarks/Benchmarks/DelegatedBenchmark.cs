using System;

namespace Benchmarks.Benchmarks
{
    public class DelegatedBenchmark : IBenchmark
    {
        public string Name { get; }

        private readonly Action setUp;
        private readonly Func<IBenchmarkingResult> run;
        private readonly Action tearDown;

        public DelegatedBenchmark(string name, Func<IBenchmarkingResult> run, Action tearDown = null) : this(name, null, run, tearDown) { }
        
        public DelegatedBenchmark(string name, Action setUp, Func<IBenchmarkingResult> run, Action tearDown = null)
        {
            if (run == null)
                throw new ArgumentException($"Illegal argument '{nameof(run)}': expected delegate, got null.");

            Name = name;

            this.setUp = setUp;
            this.run = run;
            this.tearDown = tearDown;
        }

        public void SetUp() => setUp?.Invoke();

        public IBenchmarkingResult Run()
        {
            return run.Invoke();
        }

        public void TearDown() => tearDown?.Invoke();
    }
}
