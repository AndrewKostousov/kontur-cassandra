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
            var averageWriteTime = writers.Select(x => x.TotalTime).Average();
            var writeThroughput = totalEventsWritten / averageWriteTime.TotalSeconds;
            var writesByThread = string.Join(", ", writers.Select(x => x.TotalEventsWritten));
            var throughputPerThread = writers.Select(x => x.TotalEventsWritten/x.TotalTime.TotalSeconds).Sum();

            return $"Average write latency: {averageWriteLatency.TotalMilliseconds} ms\n" +
                   $"Average write throughput: {writeThroughput} ev/s\n" +
                   $"Write throughput per thread: {throughputPerThread} ev/s\n" +
                   $"Events written: {totalEventsWritten}\n" +
                   $"Events written by single thread: {writesByThread}";
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
            var throughputPerThread = readers.Select(x => x.TotalEventsRead / x.TotalTime.TotalSeconds).Sum();

            return $"Average read latency: {averageReadLatency.TotalMilliseconds} ms\n" +
                   $"Average read throughput: {readThroughput} ev/s\n" +
                   $"Read throughput per thread: {throughputPerThread} ev/s\n" +
                   $"Reads count: {totalReadsCount}\n" +
                   $"Events read: {totalEventsRead}\n" +
                   $"Events read by single thread: {readsByThread}";
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