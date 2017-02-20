using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Benchmarks.ReadWrite;

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

            foreach (var statistic in ReadStatistics.Zip(WriteStatistics, Tuple.Create))
            {
                var readStatistic = statistic.Item1;
                var writeStatistic = statistic.Item2;

                if (readStatistic.WorkersCount != 0)
                    statistics.Append("=== Read statistics ===\r\n\r\n" + readStatistic.CreateReport());
                if (writeStatistic.WorkersCount != 0)
                    statistics.Append("=== Write statistics ===\r\n\r\n" + writeStatistic.CreateReport());
            }

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