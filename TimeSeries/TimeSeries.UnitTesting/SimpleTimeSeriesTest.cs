using System;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class TimeSeriesTestSequential : CommonTimeSeriesTestSequential
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new SimpleTimeSeries(controller.Table);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }

    [TestFixture]
    public class TimeSeriesTestParallel : CommonTimeSeriesTestParallel
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new SimpleTimeSeries(controller.Table);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }

    [TestFixture]
    public class TimeSeriesTestWrite : CommonTimeSeriesTestWrite
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new SimpleTimeSeries(controller.Table);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }
}
