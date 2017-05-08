using System.IO;
using Benchmarks.Benchmarks;

namespace Benchmarks.Runners
{
    class JsonBenchmarkLogger : BenchmarksLogger
    {
        private static string FormatFileNameFor(BenchmarksFixture fixture, IBenchmark benchmark)
        {
            var directoryName = $"Raw data\\{fixture.Name}";

            Directory.CreateDirectory(directoryName);

            return $"{directoryName}\\{benchmark.Name}.json";
        }

        public override void LogBenchmarkFinished(BenchmarksFixture fixture, IBenchmark benchmark, IBenchmarkingResult result)
        {
            var fileToWrite = FormatFileNameFor(fixture, benchmark);

            File.Delete(fileToWrite);

            result.SerializeJson(File.OpenWrite(fileToWrite));
        }
    }
}
