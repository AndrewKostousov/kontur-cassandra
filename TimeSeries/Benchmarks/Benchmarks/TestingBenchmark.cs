using System.Linq;
using System.Threading;
using CassandraTimeSeries;
using Commons.TimeBasedUuid;
using SKBKontur.Catalogue.CassandraStorageCore.CqlCore;

namespace Benchmarks.Benchmarks
{
    public class ReadBenchmark : Benchmark
    {
        protected override int IterationsCount => 100;
        public override string Name => "TimeSeries - Read By TimeGuid";

        private TimeSeries series;
        private TimeGuid start;
        private TimeGuid end;

        protected override void OnPrepare()
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

        protected override void OnRun()
        {
            series.ReadRange(start, end, 1);
        }
    }

    public class WriteBenchmark : Benchmark
    {
        protected override int IterationsCount => 100;
        public override string Name => "TimeSeries - Write";

        private TimeSeries series;

        protected override void OnPrepare()
        {
            var database = new DatabaseWrapper("test");
            series = new TimeSeries(database.Table);
        }

        protected override void OnRun()
        {
            series.Write(new Event(TimeGuid.NowGuid()));
        }
    }

    public class TestingBenchmark : Benchmark
    {
        protected override int IterationsCount => 100;
        public override string Name => "Testing benchmark";

        protected override void OnPrepare()
        {

        }

        protected override void OnRun()
        {
            Thread.Sleep(2);
        }
    }
}