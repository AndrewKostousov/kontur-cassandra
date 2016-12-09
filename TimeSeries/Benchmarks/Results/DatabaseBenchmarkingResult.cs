using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Benchmarks.ReadWrite;
using Metrics;
using Metrics.MetricData;
using Metrics.Reporters;

namespace Benchmarks.Results
{
    class DatabaseBenchmarkingResult : IBenchmarkingResult
    {
        private readonly IEnumerable<BenchmarkEventReader> readers;
        private readonly IEnumerable<BenchmarkEventWriter> writers;

        public ReadStatistics ReadStatistics { get; }
        public WriteStatistics WriteStatistics { get; }

        public DatabaseBenchmarkingResult(List<BenchmarkEventReader> readers, List<BenchmarkEventWriter> writers)
        {
            this.readers = readers;
            this.writers = writers;
            
            ReadStatistics = new ReadStatistics(readers);
            WriteStatistics = new WriteStatistics(writers);
        }

        public string CreateReport()
        {
            var totalEventsRead = readers.Sum(x => x.TotalOperationsCount());
            var totalEventsWritten = writers.Sum(x => x.TotalOperationsCount());

            var statistics = "\n";

            if (totalEventsRead != 0)
                statistics += "Read statistics:\n\n" + ReadStatistics.CreateReport();
            if (totalEventsWritten != 0 && totalEventsRead != 0)
                statistics += "\n\n";
            if (totalEventsWritten != 0)
                statistics += "Write statistics:\n\n" + WriteStatistics.CreateReport();

            return statistics;
        }

        public IBenchmarkingResult Update(IBenchmarkingResult otherResult)
        {
            var other = otherResult as DatabaseBenchmarkingResult;
            if (other == null)
                return this;
            
            return new DatabaseBenchmarkingResult(
                readers.Union(other.readers).ToList(), 
                writers.Union(other.writers).ToList());
        }
    }
}