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
        private DatabaseWrapper database;
        internal TimeSeries Series;
        internal ReadersWritersPool Pool;

        internal IEnumerable<BenchmarkEventReader> Readers;
        internal IEnumerable<BenchmarkEventWriter> Writers;

        [BenchmarkClassSetUp]
        public void ClassSetUp()
        {
            database = new DatabaseWrapper("test");
            Series = new TimeSeries(database.Table);
            Series.Write(new Event(TimeGuid.NowGuid()));
        }
        
        [BenchmarkClassTearDown]
        public void ClassTearDown()
        {
            database.Dispose();
        }
        
        [BenchmarkMethod(executionsCount:1, result:nameof(Result))]
        public void Latency_Check()
        {
            Pool.Start();
            Thread.Sleep(10000);
            Pool.Stop();
        }

        public IBenchmarkingResult Result()
        {
            return new DatabaseBenchmarkingResult(Readers, Writers);
        }
    }
}