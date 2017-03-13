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
        [Serializable]
        internal class SerializableStatistics
        {
            [DataMember] public ReadStatistics Read;
            [DataMember] public WriteStatistics Write;
        }

        private readonly SerializableStatistics statistics = new SerializableStatistics();

        public DatabaseBenchmarkingResult(IReadOnlyList<BenchmarkEventReader> readers, IReadOnlyList<BenchmarkEventWriter> writers)
        {
            statistics.Read = new ReadStatistics(readers);
            statistics.Write = new WriteStatistics(writers);
        }

        public void SerializeJson(Stream stream)
        {
            new DataContractJsonSerializer(typeof(SerializableStatistics)).WriteObject(stream, statistics);
        }

        public string CreateReport()
        {
            var statisticsString = new StringBuilder();

            var nl = Environment.NewLine;

            if (statistics.Read.WorkersCount != 0)
                statisticsString.Append($"Read statistics:{nl}{nl}" + statistics.Read.CreateReport());
            if (statistics.Write.WorkersCount != 0)
                statisticsString.Append($"Write statistics:{nl}{nl}" + statistics.Write.CreateReport());

            return statisticsString.ToString();
        }
    }
}