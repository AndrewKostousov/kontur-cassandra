using System;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class SimpleTimeSeriesTestSequential : CommonTimeSeriesTestSequential
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new SimpleTimeSeries(controller.Table, controller.BulkTable);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }

    [TestFixture]
    public class SimpleTimeSeriesTestParallel : CommonTimeSeriesTestParallel
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new SimpleTimeSeries(controller.Table, controller.BulkTable);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }

    [TestFixture]
    public class SimpleTimeSeriesTestWrite : CommonTimeSeriesTestWrite
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new SimpleTimeSeries(controller.Table, controller.BulkTable);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }
}
