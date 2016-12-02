﻿using System;
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
            var averageReadLatency = readers
                .Select(x => x.AverageLatency)
                .Average();

            var averageWriteLatency = writers
                .Select(x => x.AverageLatency)
                .Average();

            var totalReadTime = readers.Select(x => x.TotalTime).Average();
            var totalWriteTime = writers.Select(x => x.TotalTime).Average();

            var totalReadsCount = readers.Sum(x => x.TotalReadsCount);
            var totalEventsRead = readers.Sum(x => x.TotalEventsRead);
            var totalEventsWritten = writers.Sum(x => x.TotalEventsWritten);

            var readThroughput = totalEventsRead / totalReadTime.TotalSeconds;
            var writeThroughput = totalEventsWritten / totalWriteTime.TotalSeconds;
            
            var statistics = "\n";

            if (totalEventsRead != 0)
                statistics +=
                    $"Average read latency: {averageReadLatency}\n" +
                    $"Total reads count: {totalReadsCount}\n" +
                    $"Total events read: {totalEventsRead}\n" +
                    $"Total events read by single thread: {string.Join(", ", readers.Select(x => x.TotalEventsRead))}\n" +
                    $"Read throughput: {readThroughput} events per second";

            if (totalEventsWritten != 0)
                statistics += "\n\n" +
                    $"Average write latency: {averageWriteLatency}\n" +
                    $"Total events written: {totalEventsWritten}\n" +
                    $"Total events written by single thread: {string.Join(", ", writers.Select(x => x.TotalEventsWritten))}\n" +
                    $"Write throughput: {writeThroughput} events per second";

            return statistics;
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