using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using CassandraTimeSeries.Utils;

namespace Benchmarks.Benchmarks
{
    // ReSharper disable once UnusedMember.Global
    //public class SimpleSeriesReadAndWriteBenchmark : BaseTimeSeriesBenchmark<SimpleTimeSeriesDatabaseController>
    //{
    //    public override string Name => $"{nameof(SimpleTimeSeries)}";
    //    protected override ITimeSeries TimeSeriesFactory(SimpleTimeSeriesDatabaseController c) => new SimpleTimeSeries(c, new TimeLinePartitioner());
    //}

    // ReSharper disable once UnusedMember.Global
    public class CasSeriesReadAndWriteBenchmark : BaseTimeSeriesBenchmark<CasTimeSeriesDatabaseController>
    {
        public override string Name => $"{nameof(CasTimeSeries)} with single write";
        protected override ITimeSeries TimeSeriesFactory(CasTimeSeriesDatabaseController c) => new CasTimeSeries(c, new TimeLinePartitioner());
    }

    // ReSharper disable once UnusedMember.Global
    public class AllBoxEventSeriesBenchmark : BaseTimeSeriesBenchmark<EdiTimeSeriesDatabaseController>
    {
        public override string Name => $"{nameof(EdiTimeSeriesWrapper)} with single write";
        protected override ITimeSeries TimeSeriesFactory(EdiTimeSeriesDatabaseController c) => new EdiTimeSeriesWrapper(c, new TimeLinePartitioner());
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
