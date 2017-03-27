using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class EdiSeriesTestSequential : CommonTimeSeriesTestSequential
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new EdiTimeSeriesWrapper(controller.Cluster, new TimeLinePartitioner());
        private readonly EdiTimeSeriesDatabaseController controller = new EdiTimeSeriesDatabaseController();
    }

    [TestFixture]
    public class EdiSeriesTestParallel : CommonTimeSeriesTestParallel
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new EdiTimeSeriesWrapper(controller.Cluster, new TimeLinePartitioner());
        private readonly EdiTimeSeriesDatabaseController controller = new EdiTimeSeriesDatabaseController();
    }

    [TestFixture]
    public class EdiSeriesTestWrite : CommonTimeSeriesTestWrite
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new EdiTimeSeriesWrapper(controller.Cluster, new TimeLinePartitioner());
        private readonly EdiTimeSeriesDatabaseController controller = new EdiTimeSeriesDatabaseController();
    }
}
