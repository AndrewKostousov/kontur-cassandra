using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Benchmarks.ReadWrite;
using System.IO;

namespace Benchmarks.Results
{
    class DatabaseBenchmarkingResult : IBenchmarkingResult
    {
        [DataContract]
        internal class SerializableStatistics
        {
            [DataMember] public ReadStatistics Readers;
            [DataMember] public WriteStatistics Writers;
        }

        private readonly SerializableStatistics statistics = new SerializableStatistics();

        public DatabaseBenchmarkingResult(IReadOnlyList<BenchmarkEventReader> readers, IReadOnlyList<BenchmarkEventWriter> writers)
        {
            statistics.Readers = new ReadStatistics(readers);
            statistics.Writers = new WriteStatistics(writers);
        }

        public void SerializeJson(Stream stream)
        {
            new DataContractJsonSerializer(typeof(SerializableStatistics)).WriteObject(stream, statistics);
        }

        public string CreateReport()
        {
            var statisticsString = new StringBuilder();

            var nl = Environment.NewLine;

            if (statistics.Readers.WorkersCount != 0)
                statisticsString.Append($"Read statistics:{nl}{nl}" + statistics.Readers.CreateReport());
            if (statistics.Writers.WorkersCount != 0)
                statisticsString.Append($"Write statistics:{nl}{nl}" + statistics.Writers.CreateReport());

            return statisticsString.ToString();
        }
    }
}