using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using CassandraTimeSeries.Utils;

namespace Benchmarks.Benchmarks
{
    // ReSharper disable once UnusedMember.Global
    public class CasSeriesReadAndWriteBenchmark : BaseTimeSeriesBenchmark
    {
        public override string Name => $"{nameof(CasTimeSeries)} (bulk write, 4 writers)";

        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(controller.EventsTable, controller.SyncTable, new TimeLinePartitioner());
        private readonly CasTimeSeriesDatabaseController controller = new CasTimeSeriesDatabaseController();

        protected override WriterSettings WriterSettings => new WriterSettings {BulkSize = 10};


        protected override int WritersCount => 4;
    }

    // ReSharper disable once UnusedMember.Global
    public class A : CasSeriesReadAndWriteBenchmark
    {
        public override string Name => $"{nameof(EdiTimeSeriesWrapper)} (bulk write, 1 writer)";
        
        protected override int WritersCount => 1;
    }

    // ReSharper disable once UnusedMember.Global
    public class B : CasSeriesReadAndWriteBenchmark
    {
        public override string Name => $"{nameof(EdiTimeSeriesWrapper)} (bulk write, 8 writers)";

        
        protected override int WritersCount => 8;
    }
}
