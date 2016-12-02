using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarks.ReadWrite;

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
            var averageReadLatency = TimeSpan.FromTicks((long)readers
                .Select(x => x.AverageLatency)
                .Average(x => x.Ticks));

            var totalReadsCount = readers.Sum(x => x.TotalReadsCount);

            var averageWriteLatency = TimeSpan.FromTicks((long)writers
                .Select(x => x.AverageLatency)
                .Average(x => x.Ticks));

            var totalWritesCount = writers.Sum(x => x.TotalEventsWritten);

            return $"Average read latency: {averageReadLatency}\n" +
                   $"Average write latency: {averageWriteLatency}\n" +
                   $"Total reads count: {totalReadsCount}\n" +
                   $"Total writes count: {totalWritesCount}\n";
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