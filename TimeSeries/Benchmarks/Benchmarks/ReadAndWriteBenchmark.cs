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
        // TODO: readers are not fast enough to read all written events
        public ReadAndWriteBenchmark() : base(readersCount:4, writersCount:4) { }
    }

    [BenchmarkClass]
    public class ReadOnlyBenchmark : TimeSeriesBenchmark
    {
        public ReadOnlyBenchmark() : base(readersCount: 4, writersCount: 0) { }

        [BenchmarkSetUp]
        public override void SetUp()
        {
            base.SetUp();

            for(var i = 0; i < 10000; ++i)
                Series.Write(new Event(TimeGuid.NowGuid()));
        }
    }

    [BenchmarkClass]
    public class WriteOnlyBenchmark : TimeSeriesBenchmark
    {
        public WriteOnlyBenchmark() : base(readersCount: 0, writersCount: 4) { }
    }
}