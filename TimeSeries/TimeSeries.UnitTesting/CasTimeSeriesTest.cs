using System;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class CasTimeSeriesTestSequential : CommonTimeSeriesTestSequential
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(controller.Table, controller.SyncTable);
        private readonly CasTimeSeriesDatabaseController controller = new CasTimeSeriesDatabaseController();
    }

    [TestFixture]
    public class CasTimeSeriesTestParallel : CommonTimeSeriesTestParallel
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(controller.Table, controller.SyncTable);
        private readonly CasTimeSeriesDatabaseController controller = new CasTimeSeriesDatabaseController();
    }

    [TestFixture]
    public class CasTimeSeriesTestWrite : CommonTimeSeriesTestWrite
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(controller.Table, controller.SyncTable);
        private readonly CasTimeSeriesDatabaseController controller = new CasTimeSeriesDatabaseController();
    }
}
