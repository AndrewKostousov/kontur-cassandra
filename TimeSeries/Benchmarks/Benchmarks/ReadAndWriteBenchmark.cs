using System.ComponentModel;
using System.Linq;
using Benchmarks.ReadWrite;
using Benchmarks.Reflection;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using Commons.TimeBasedUuid;

namespace Benchmarks.Benchmarks
{
    [BenchmarkClass]
    public class ReadAndWriteBenchmark : TimeSeriesBenchmark
    {
        protected override int ReadersCount => 4;
        protected override int WritersCount => 4;
    }

    [BenchmarkClass]
    public class ReadOnlyBenchmark : TimeSeriesBenchmark
    {
        protected override int ReadersCount => 4;
        protected override int WritersCount => 0;
        protected readonly int PreloadEventsCount = 50000;

        [BenchmarkSetUp]
        public override void SetUp()
        {
            base.SetUp();

            for(var i = 0; i < PreloadEventsCount; ++i)
                Series.Write(new Event(TimeGuid.NowGuid()));
        }
    }

    [BenchmarkClass]
    public class WriteOnlyBenchmark : TimeSeriesBenchmark
    {
        protected override int ReadersCount => 0;
        protected override int WritersCount => 4;
    }
}