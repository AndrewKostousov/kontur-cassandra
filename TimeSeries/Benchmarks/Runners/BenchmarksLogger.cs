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
        public virtual void LogFixtureSetup(BenchmarksFixture fixture) { }
        public virtual void LogFixtureTearDown(BenchmarksFixture fixture) { }
        public virtual void LogBenchmarkSetup(BenchmarksFixture fixture, IBenchmark benchmark) { }
        public virtual void LogBenchmarkStarted(BenchmarksFixture fixture, IBenchmark benchmark) { }
        public virtual void LogBenchmarkFinished(BenchmarksFixture fixture, IBenchmark benchmark, IBenchmarkingResult result) { }
        public virtual void LogBenchmarkTearDown(BenchmarksFixture fixture, IBenchmark benchmark) { }
        public virtual void LogAllFixturesFinished() { }
    }
}