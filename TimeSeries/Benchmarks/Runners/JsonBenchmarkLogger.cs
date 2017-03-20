using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Benchmarks.Benchmarks;

namespace Benchmarks.Runners
{
    class JsonBenchmarkLogger : BenchmarksLogger
    {
        private static string FormatFileNameFor(BenchmarksFixture fixture, Benchmark benchmark)
        {
            var directoryName = $"Raw data\\{fixture.Name}";

            Directory.CreateDirectory(directoryName);

            return $"{directoryName}\\{benchmark.Name}.json";
        }

        public override void LogBenchmarkFinished(BenchmarksFixture fixture, Benchmark benchmark, IBenchmarkingResult result)
        {
            result.SerializeJson(File.OpenWrite(FormatFileNameFor(fixture, benchmark)));
        }
    }
}
