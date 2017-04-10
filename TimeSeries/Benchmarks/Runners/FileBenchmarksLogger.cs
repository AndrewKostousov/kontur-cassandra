using System.IO;
using Benchmarks.Benchmarks;

namespace Benchmarks.Runners
{
    class FileBenchmarksLogger : BenchmarksLogger
    {
        private static string FormatFileNameFor(BenchmarksFixture fixture)
        {
            const string directoryName = "Results";

            Directory.CreateDirectory(directoryName);

            return $"{directoryName}\\{fixture.Name}.txt";
        }

        public override void LogFixtureBegan(BenchmarksFixture fixture) =>
            File.WriteAllText(FormatFileNameFor(fixture), CreateBigTitle($"   {fixture.Name}"));

        public override void LogBenchmarkStarted(BenchmarksFixture fixture, IBenchmark benchmark) =>
            File.AppendAllText(FormatFileNameFor(fixture), $"{NewLine}== {benchmark.Name} ".PadRight(100, '=') + NewLine);

        public override void LogBenchmarkFinished(BenchmarksFixture fixture, IBenchmark benchmark, IBenchmarkingResult result) =>
            File.AppendAllText(FormatFileNameFor(fixture), NewLine + result.CreateReport());
    }
}