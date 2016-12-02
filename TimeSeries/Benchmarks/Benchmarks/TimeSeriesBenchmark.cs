using System.Collections.Generic;
using System.Threading;
using Benchmarks.ReadWrite;
using Benchmarks.Reflection;
using Benchmarks.Results;
using CassandraTimeSeries.Model;
using Commons.TimeBasedUuid;

namespace Benchmarks.Benchmarks
{
    [BenchmarkClass]
    public abstract class TimeSeriesBenchmark
    {
        internal DatabaseWrapper Database;
        internal TimeSeries Series;
        internal ReadersWritersPool Pool;

        internal List<BenchmarkEventReader> Readers;
        internal List<BenchmarkEventWriter> Writers;

        [BenchmarkClassSetUp]
        public void ClassSetUp()
        {
            Database = new DatabaseWrapper("test");
            Series = new TimeSeries(Database.Table);
            Series.Write(new Event(TimeGuid.NowGuid()));
        }
        
        [BenchmarkClassTearDown]
        public void ClassTearDown()
        {
            Database.Dispose();
        }
        
        [BenchmarkMethod(executionsCount:5, result:nameof(Result))]
        public void Benchmark()
        {
            Pool.Start();
            Thread.Sleep(2000);
            Pool.Stop();
        }

        public IBenchmarkingResult Result()
        {
            return new DatabaseBenchmarkingResult(Readers, Writers);
        }
    }
}