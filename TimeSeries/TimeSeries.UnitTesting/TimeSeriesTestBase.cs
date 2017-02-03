using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    public abstract class TimeSeriesTestBase
    {
        protected DatabaseWrapper Wrapper;
        protected ITimeSeries Series;

        protected abstract ITimeSeries TimeSeriesFactory();

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            Wrapper = new DatabaseWrapper("test");
            Wrapper.Table.Drop();
            Wrapper.Table.Create();
            Series = TimeSeriesFactory();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Wrapper.Dispose();
        }
    }
}