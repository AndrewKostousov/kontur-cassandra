using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class AllBoxEventSeriesTestSequential : CommonTimeSeriesTestSequential
    {
        protected override ITimeSeries TimeSeriesFactory() => new AllBoxEventSeriesWrapper();
    }

    [TestFixture]
    public class AllBoxEventSeriesTestParallel : CommonTimeSeriesTestParallel
    {
        protected override ITimeSeries TimeSeriesFactory() => new AllBoxEventSeriesWrapper();
    }

    [TestFixture]
    public class AllBoxEventSeriesTestWrite : CommonTimeSeriesTestWrite
    {
        protected override ITimeSeries TimeSeriesFactory() => new AllBoxEventSeriesWrapper();
    }
}
