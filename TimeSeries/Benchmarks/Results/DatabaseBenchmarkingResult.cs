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
        public List<ReadStatistics> ReadStatistics { get; }
        public List<WriteStatistics> WriteStatistics { get; }

        public DatabaseBenchmarkingResult(List<BenchmarkEventReader> readers, List<BenchmarkEventWriter> writers)
        {
            ReadStatistics = new List<ReadStatistics> {new ReadStatistics(readers)};
            WriteStatistics = new List<WriteStatistics> {new WriteStatistics(writers)};
        }

        private DatabaseBenchmarkingResult(List<ReadStatistics> reads, List<WriteStatistics> writes)
        {
            ReadStatistics = reads;
            WriteStatistics = writes;
        }

        public string CreateReport()
        {
            var totalEventsRead = ReadStatistics.First().TotalEventsRead;
            var totalEventsWritten = WriteStatistics.First().TotalOperationsCount;

            var statistics = "\n";

            if (totalEventsRead != 0)
                statistics += "Read statistics:\n\n" + ReadStatistics.First().CreateReport();
            if (totalEventsWritten != 0 && totalEventsRead != 0)
                statistics += "\n\n";
            if (totalEventsWritten != 0)
                statistics += "Write statistics:\n\n" + WriteStatistics.First().CreateReport();

            return statistics;
        }

        public IBenchmarkingResult Update(IBenchmarkingResult otherResult)
        {
            var other = otherResult as DatabaseBenchmarkingResult;
            if (other == null)
                return this;
            
            return new DatabaseBenchmarkingResult(
                ReadStatistics.Union(other.ReadStatistics).ToList(), 
                WriteStatistics.Union(other.WriteStatistics).ToList());
        }
    }
}