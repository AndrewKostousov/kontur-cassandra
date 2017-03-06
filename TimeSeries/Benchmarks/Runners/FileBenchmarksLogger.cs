﻿using System.IO;
using Benchmarks.Benchmarks;

namespace Benchmarks.Runners
{
    class FileBenchmarksLogger : BenchmarksLogger
    {
        private static string FormatFileNameFor(BenchmarksFixture fixture)
        {
            const string resultsFilePrefix = "results for ";
            const string resultsFilePostfix = ".txt";

            return resultsFilePrefix + fixture.Name + resultsFilePostfix;
        }

        public override void LogFixtureBegan(BenchmarksFixture fixture) =>
            File.WriteAllText(FormatFileNameFor(fixture), CreateBigTitle($"   {fixture.Name}"));

        public override void LogBenchmarkStarted(BenchmarksFixture fixture, Benchmark benchmark) =>
            File.AppendAllText(FormatFileNameFor(fixture), $"{NewLine}== {benchmark.Name} ".PadRight(100, '=') + NewLine);

        public override void LogBenchmarkFinished(BenchmarksFixture fixture, Benchmark benchmark, string result) =>
            File.AppendAllText(FormatFileNameFor(fixture), NewLine + result);
    }
}