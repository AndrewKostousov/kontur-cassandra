using System;
using System.Collections.Generic;
using System.Linq;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using Commons.TimeBasedUuid;

namespace Benchmarks.Benchmarks
{
    [BenchmarkClass]
    public class DatabaseBenchmark
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

        [BenchmarkMethod(result:nameof(GetDatabaseStats))]
        public void TimeSeries_Write()
        {
            foreach (var e in eventsToWrite)
                series.Write(e);
        }

        [BenchmarkMethod(result:nameof(GetDatabaseStats))]
        public void TimeSeries_Read()
        {
            series.ReadRange(start, end, 1);
        }

        public IBenchmarkingResult GetDatabaseStats()
        {
            var eventsWritten = series.ReadRange(start, end);
            var misswrites = new HashSet<Event>(eventsToWrite);
            misswrites.ExceptWith(eventsWritten);

            return new DatabaseBenchmarkingResult(misswrites.Count);
        }
    }

    //[BenchmarkClass]
    public class TestingBenchmark
    {
        [BenchmarkClassSetUp]
        public void ClassSetUp()
        {
            Console.WriteLine($"{nameof(ClassSetUp)}");
        }

        [BenchmarkSetUp]
        public void SetUp()
        {
            Console.WriteLine($"{nameof(SetUp)}");
        }

        [BenchmarkMethod(1)]
        public void First()
        {
            Console.WriteLine($"{nameof(First)}");
        }

        [BenchmarkMethod(1, nameof(CountResult))]
        public void Second()
        {
            Console.WriteLine($"{nameof(Second)}");
        }

        [BenchmarkTearDown]
        public void TearDown()
        {
            Console.WriteLine($"{nameof(TearDown)}");
        }

        [BenchmarkClassTearDown]
        public void ClassTearDown()
        {
            Console.WriteLine($"{nameof(ClassTearDown)}");
        }
        
        public IBenchmarkingResult CountResult()
        {
            Console.WriteLine($"{nameof(CountResult)}");
            return new BenchmarkingResult(TimeSpan.FromSeconds(42));
        }
    }
}