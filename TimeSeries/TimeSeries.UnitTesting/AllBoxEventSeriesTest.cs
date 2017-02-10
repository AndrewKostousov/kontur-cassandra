using System;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class AllBoxEventSeriesTestSequential : CommonTimeSeriesTestSequential
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new AllBoxEventSeriesWrapper(controller.Cluster);
        private readonly AllBoxEventSeriesDatabaseController controller = new AllBoxEventSeriesDatabaseController();
    }

    [TestFixture]
    public class AllBoxEventSeriesTestParallel : CommonTimeSeriesTestParallel
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new AllBoxEventSeriesWrapper(controller.Cluster);
        private readonly AllBoxEventSeriesDatabaseController controller = new AllBoxEventSeriesDatabaseController();
    }

    [TestFixture]
    public class AllBoxEventSeriesTestWrite : CommonTimeSeriesTestWrite
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new AllBoxEventSeriesWrapper(controller.Cluster);
        private readonly AllBoxEventSeriesDatabaseController controller = new AllBoxEventSeriesDatabaseController();
    }
}
