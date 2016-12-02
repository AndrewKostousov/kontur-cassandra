using System.Collections.Generic;
using System.Linq;
using Benchmarks.Reflection;
using Benchmarks.Results;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using Commons.TimeBasedUuid;

namespace Benchmarks.Benchmarks
{
    //[BenchmarkClass]
    public class SimpleDatabaseBenchmark
    {
        private DatabaseWrapper database;
        private TimeSeries series;
        private TimeGuid start;
        private TimeGuid end;
        private IEnumerable<Event> eventsToWrite;

        [BenchmarkClassSetUp]
        public void ClassSetUp()
        {
            database = new DatabaseWrapper("test");
            series = new TimeSeries(database.Table);

            eventsToWrite = Enumerable.Range(0, 10)
                .Select(x => new Event(TimeGuid.NowGuid()))
                .ToList();

            start = eventsToWrite.Min(e => e.Id).ToTimeGuid();
            end = eventsToWrite.Max(e => e.Id).ToTimeGuid();
        }

        [BenchmarkClassTearDown]
        public void ClassTearDown()
        {
            database.Dispose();
        }

        [BenchmarkSetUp]
        public void SetUp()
        {
            database.Table.Truncate();
        }

        [BenchmarkMethod]
        public void TimeSeries_Write()
        {
            foreach (var e in eventsToWrite)
                series.Write(e);
        }

        [BenchmarkMethod]
        public void TimeSeries_Read()
        {
            series.ReadRange(start, end, 1);
        }
    }
}