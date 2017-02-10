using System;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    public abstract class TimeSeriesTestBase
    {
        protected abstract IDatabaseController Database { get; }
        protected ITimeSeries Series { get; private set; }

        protected abstract ITimeSeries TimeSeriesFactory();

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            Database.SetUpSchema();
            Series = TimeSeriesFactory();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Database.TearDownSchema();
        }
    }
}