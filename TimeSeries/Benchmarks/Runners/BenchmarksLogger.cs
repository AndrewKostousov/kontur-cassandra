using System;
using Benchmarks.Benchmarks;

namespace Benchmarks.Runners
{
    abstract class BenchmarksLogger
    {
        protected static readonly string NewLine = Environment.NewLine;
        protected static readonly string Separator = NewLine.PadLeft(100, '=');

        protected string CreateBigTitle(string titleText) => $"{Separator}{NewLine}{titleText}{NewLine}{NewLine}{Separator}";

        public virtual void LogFixtureBegan(BenchmarksFixture fixture) { }
        public virtual void LogBenchmarkSetup(BenchmarksFixture fixture, Benchmark benchmark) { }
        public virtual void LogBenchmarkStarted(BenchmarksFixture fixture, Benchmark benchmark) { }
        public virtual void LogBenchmarkFinished(BenchmarksFixture fixture, Benchmark benchmark, string result) { }
        public virtual void LogBenchmarkTearDown(BenchmarksFixture fixture, Benchmark benchmark) { }
        public virtual void LogAllFixturesFinished() { }
    }
}