using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var statistics = new StringBuilder();

            foreach (var readStatistic in ReadStatistics.Where(s => s.WorkersCount != 0))
                statistics.Append("Read statistics:\n\n" + readStatistic.CreateReport());

            foreach (var writeStatistic in WriteStatistics.Where(s => s.WorkersCount != 0))
                statistics.Append("Write statistics:\n\n" + writeStatistic.CreateReport());

            return statistics.ToString();
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