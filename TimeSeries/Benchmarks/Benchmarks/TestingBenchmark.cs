using System;
using System.Collections.Generic;
using System.Linq;
using CassandraTimeSeries.Model;
using Commons.TimeBasedUuid;

namespace Benchmarks.Benchmarks
{
    [BenchmarkClass]
    public class DatabaseBenchmark
    {
        private TimeSeries series;
        private TimeGuid start;
        private TimeGuid end;

        [BenchmarkClassSetUp]
        public void SetUp()
        {
            var database = new DatabaseWrapper("test");
            series = new TimeSeries(database.Table);

            var events = Enumerable.Range(0, 10)
                .Select(x => new Event(TimeGuid.NowGuid()))
                .ToList();

            start = events.Min(e => e.Id).ToTimeGuid();
            end = events.Max(e => e.Id).ToTimeGuid();

            events.ForEach(e => series.Write(e));
        }
        
        [BenchmarkMethod]
        public void TimeSeries_Write()
        {
            series.Write(new Event(TimeGuid.NowGuid()));
        }

        [BenchmarkMethod]
        public void TimeSeries_Read()
        {
            series.ReadRange(start, end, 1);
        }
    }

    [BenchmarkClass]
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

        [BenchmarkMethod(1)]
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

        [BenchmarkResult]
        public IBenchmarkingResult Result()
        {
            Console.WriteLine($"{nameof(Result)}");
            return new BenchmarkingResult(TimeSpan.FromSeconds(42));
        }
    }
}