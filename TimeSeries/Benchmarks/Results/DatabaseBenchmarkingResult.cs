using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Benchmarks.ReadWrite;
using Commons;
using Metrics;
using Metrics.MetricData;
using Metrics.Reporters;

namespace Benchmarks.Results
{
    class DatabaseBenchmarkingResult : IBenchmarkingResult
    {
        private readonly IEnumerable<BenchmarkEventReader> readers;
        private readonly IEnumerable<BenchmarkEventWriter> writers;

        public DatabaseBenchmarkingResult(IEnumerable<BenchmarkEventReader> readers, IEnumerable<BenchmarkEventWriter> writers)
        {
            this.readers = readers;
            this.writers = writers;
        }

        public string CreateReport()
        {
            var totalEventsRead = readers.Sum(x => x.TotalEventsRead);
            var totalEventsWritten = writers.Sum(x => x.TotalEventsWritten);

            var statistics = "\n";

            if (totalEventsRead != 0)
                statistics += GetReadStatistics();
            if (totalEventsWritten != 0 && totalEventsRead != 0)
                statistics += "\n\n";
            if (totalEventsWritten != 0)
                statistics += GetWriteStatistics();

            return statistics;
        }

        private string GetWriteStatistics()
        {
            var averageWriteLatency = writers
                .Select(x => x.AverageLatency)
                .Average();

            var totalEventsWritten = writers.Sum(x => x.TotalEventsWritten);
            var totalWriteTime = writers.Select(x => x.TotalTime).Average();
            var writeThroughput = totalEventsWritten / totalWriteTime.TotalSeconds;
            var writesByThread = string.Join(", ", writers.Select(x => x.TotalEventsWritten));

            return $"Average write latency: {averageWriteLatency}\n" +
                   $"Total events written: {totalEventsWritten}\n" +
                   $"Total events written by single thread: {writesByThread}\n" +
                   $"Write throughput: {writeThroughput} events per second";
        }

        private string GetReadStatistics()
        {
            var averageReadLatency = readers
                .Select(x => x.AverageLatency)
                .Average();

            var totalEventsRead = readers.Sum(x => x.TotalEventsRead);
            var totalReadTime = readers.Select(x => x.TotalTime).Average();
            var totalReadsCount = readers.Sum(x => x.TotalReadsCount);
            var readThroughput = totalEventsRead / totalReadTime.TotalSeconds;
            var readsByThread = string.Join(", ", readers.Select(x => x.TotalEventsRead));

            return $"Average read latency: {averageReadLatency}\n" +
                   $"Total reads count: {totalReadsCount}\n" +
                   $"Total events read: {totalEventsRead}\n" +
                   $"Total events read by single thread: {readsByThread}\n" +
                   $"Read throughput: {readThroughput} events per second";
        }

        public IBenchmarkingResult Update(IBenchmarkingResult otherResult)
        {
            var other = otherResult as DatabaseBenchmarkingResult;
            if (other == null)
                return this;
            
            return new DatabaseBenchmarkingResult(readers.Union(other.readers), writers.Union(other.writers));
        }
    }
}