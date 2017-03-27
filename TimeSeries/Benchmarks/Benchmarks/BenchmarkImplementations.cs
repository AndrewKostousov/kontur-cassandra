using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using CassandraTimeSeries.Utils;

namespace Benchmarks.Benchmarks
{
    // ReSharper disable once UnusedMember.Global
    public class SimpleSeriesReadAndWriteBenchmark : BaseTimeSeriesBenchmark
    {
        public override string Name => $"{nameof(SimpleTimeSeries)}";

        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new SimpleTimeSeries(controller.EventsTable, new TimeLinePartitioner());
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }

    // ReSharper disable once UnusedMember.Global
    public class CasSeriesReadAndWriteBenchmark : BaseTimeSeriesBenchmark
    {
        public override string Name => $"{nameof(CasTimeSeries)} with single write";

        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(controller.EventsTable, controller.SyncTable, new TimeLinePartitioner());
        private readonly CasTimeSeriesDatabaseController controller = new CasTimeSeriesDatabaseController();
    }

    // ReSharper disable once UnusedMember.Global
    public class AllBoxEventSeriesBenchmark : BaseTimeSeriesBenchmark
    {
        public override string Name => $"{nameof(EdiTimeSeriesWrapper)} with single write";

        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new EdiTimeSeriesWrapper(controller.Cluster, new TimeLinePartitioner());
        private readonly EdiTimeSeriesDatabaseController controller = new EdiTimeSeriesDatabaseController();
    }

    // ReSharper disable once UnusedMember.Global
    public class CasSeriesReadAndWriteBulkBenchmark : CasSeriesReadAndWriteBenchmark
    {
        public override string Name => $"{nameof(CasTimeSeries)} with bulk write";
        protected override WriterSettings WriterSettings { get; } = new WriterSettings {BulkSize = 10};
    }

    // ReSharper disable once UnusedMember.Global
    public class AllBoxEventSeriesBulkBenchmark : AllBoxEventSeriesBenchmark
    {
        public override string Name => $"{nameof(EdiTimeSeriesWrapper)} with bulk write";
        protected override WriterSettings WriterSettings { get; } = new WriterSettings {BulkSize = 10};
    }
}
