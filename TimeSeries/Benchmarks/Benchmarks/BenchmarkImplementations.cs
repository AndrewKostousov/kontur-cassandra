using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;

namespace Benchmarks.Benchmarks
{
    // ReSharper disable once UnusedMember.Global
    public class SimpleSeriesReadAndWriteBenchmark : BaseTimeSeriesBenchmark
    {
        public override string Name => $"{nameof(SimpleTimeSeries)} benchmark";

        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new SimpleTimeSeries(controller.Table);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }

    // ReSharper disable once UnusedMember.Global
    public class CasSeriesReadAndWriteBenchmark : BaseTimeSeriesBenchmark
    {
        public override string Name => $"{nameof(CasTimeSeries)} benchmark";

        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(controller.Table, controller.SyncTable);
        private readonly CasTimeSeriesDatabaseController controller = new CasTimeSeriesDatabaseController();
    }
}
