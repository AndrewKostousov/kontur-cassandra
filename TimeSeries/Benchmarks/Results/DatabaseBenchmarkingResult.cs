using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Benchmarks.ReadWrite;

namespace Benchmarks.Results
{
    class DatabaseBenchmarkingResult : IBenchmarkingResult
    {
        private readonly ReadStatistics readStatistics;
        private readonly WriteStatistics writeStatistics;

        public DatabaseBenchmarkingResult(IReadOnlyList<BenchmarkEventReader> readers, IReadOnlyList<BenchmarkEventWriter> writers)
        {
            readStatistics = new ReadStatistics(readers);
            writeStatistics = new WriteStatistics(writers);
        }

        public string CreateReport()
        {
            var statistics = new StringBuilder();

            var nl = Environment.NewLine;

            if (readStatistics.WorkersCount != 0)
                statistics.Append($"Read statistics:{nl}{nl}" + readStatistics.CreateReport());
            if (writeStatistics.WorkersCount != 0)
                statistics.Append($"Write statistics:{nl}{nl}" + writeStatistics.CreateReport());

            return statistics.ToString();
        }
    }
}