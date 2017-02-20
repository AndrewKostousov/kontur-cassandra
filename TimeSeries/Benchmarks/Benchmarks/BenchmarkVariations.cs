using Benchmarks.Reflection;
using CassandraTimeSeries.Model;
using Commons.TimeBasedUuid;

namespace Benchmarks.Benchmarks
{
    [BenchmarkClass]
    public abstract class ReadAndWriteBenchmark : BaseTimeSeriesBenchmark
    {
        protected override int ReadersCount => 4;
        protected override int WritersCount => 4;
    }

    [BenchmarkClass]
    public abstract class ReadOnlyBenchmark : BaseTimeSeriesBenchmark
    {
        protected override int ReadersCount => 4;
        protected override int WritersCount => 0;
        protected readonly int PreloadedEventsCount = 5000;

        [BenchmarkSetUp]
        public override void SetUp()
        {
            base.SetUp();

            for(var i = 0; i < PreloadedEventsCount; ++i)
                Series.WriteWithoutSync(new Event(TimeGuid.NowGuid(), new EventProto()));
        }
    }

    [BenchmarkClass]
    public abstract class WriteOnlyBenchmark : BaseTimeSeriesBenchmark
    {
        protected override int ReadersCount => 0;
        protected override int WritersCount => 4;
    }
}