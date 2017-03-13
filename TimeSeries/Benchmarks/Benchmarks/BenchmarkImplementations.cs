using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;

namespace Benchmarks.Benchmarks
{
    // ReSharper disable once UnusedMember.Global
    public class SimpleSeriesReadAndWriteBenchmark : BaseTimeSeriesBenchmark
    {
        public override string Name => $"{nameof(SimpleTimeSeries)}";

        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new SimpleTimeSeries(controller.EventsTable);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }

    // ReSharper disable once UnusedMember.Global
    public class CasSeriesReadAndWriteBenchmark : BaseTimeSeriesBenchmark
    {
        public override string Name => $"{nameof(CasTimeSeries)} with single write";

        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(controller.EventsTable, controller.SyncTable);
        private readonly CasTimeSeriesDatabaseController controller = new CasTimeSeriesDatabaseController();
    }

    // ReSharper disable once UnusedMember.Global
    public class AllBoxEventSeriesBenchmark : BaseTimeSeriesBenchmark
    {
        public override string Name => $"{nameof(AllBoxEventSeriesWrapper)} with single write";
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new AllBoxEventSeriesWrapper(controller.Cluster);
        private readonly AllBoxEventSeriesDatabaseController controller = new AllBoxEventSeriesDatabaseController();
    }

    // ReSharper disable once UnusedMember.Global
    public class CasSeriesReadAndWriteBulkBenchmark : CasSeriesReadAndWriteBenchmark
    {
        public override string Name => $"{nameof(CasTimeSeries)} with bulk write";
        protected override WriterSettings WriterSettings { get; } = new WriterSettings { BulkSize = 10 };
    }

    // ReSharper disable once UnusedMember.Global
    public class AllBoxEventSeriesBulkBenchmark : AllBoxEventSeriesBenchmark
    {
        public override string Name => $"{nameof(AllBoxEventSeriesWrapper)} with bulk write";
        protected override WriterSettings WriterSettings { get; } = new WriterSettings { BulkSize = 10 };
    }
}
