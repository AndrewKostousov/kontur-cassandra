using System;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using Commons.Logging;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    public abstract class TimeSeriesTestBase<TDatabaseController>
        where TDatabaseController : IDatabaseController, new ()
    {
        protected ITimeSeries Series { get; private set; }

        protected IDatabaseController Database { get; } = new TDatabaseController();

        protected abstract ITimeSeries TimeSeriesFactory(TDatabaseController controller);

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            Logging.SetUp();

            Database.SetUpSchema();
            Series = TimeSeriesFactory(new TDatabaseController());
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Database.Dispose();
        }
    }
}