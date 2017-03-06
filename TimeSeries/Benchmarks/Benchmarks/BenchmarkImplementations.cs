using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;

namespace Benchmarks.Benchmarks
{
    // ReSharper disable once UnusedMember.Global
    public class SimpleSeriesReadAndWriteBenchmark : BaseTimeSeriesBenchmark
    {
        public override string Name => $"{nameof(SimpleTimeSeries)} benchmark";

        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new SimpleTimeSeries(controller.Table, controller.BulkTable);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }

    // ReSharper disable once UnusedMember.Global
    public class CasSeriesReadAndWriteBenchmark : BaseTimeSeriesBenchmark
    {
        public override string Name => $"{nameof(CasTimeSeries)} benchmark";

        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(controller.Table, controller.BulkTable, controller.SyncTable);
        private readonly CasTimeSeriesDatabaseController controller = new CasTimeSeriesDatabaseController();
    }

    // ReSharper disable once UnusedMember.Global
    public class AllBoxEventSeriesBenchmark : BaseTimeSeriesBenchmark
    {
        public override string Name => $"{nameof(AllBoxEventSeriesWrapper)} benchmark";
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new AllBoxEventSeriesWrapper(controller.Cluster);
        private readonly AllBoxEventSeriesDatabaseController controller = new AllBoxEventSeriesDatabaseController();
    }
}
