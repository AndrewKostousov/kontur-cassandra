using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class TimeSeriesTestSequential : CommonTimeSeriesTestSequential
    {
        protected override ITimeSeries TimeSeriesFactory() => new TimeSeries(Wrapper.Table);
    }

    [TestFixture]
    public class TimeSeriesTestParallel : CommonTimeSeriesTestParallel
    {
        protected override ITimeSeries TimeSeriesFactory() => new TimeSeries(Wrapper.Table);
    }

    [TestFixture]
    public class TimeSeriesTestWrite : CommonTimeSeriesTestWrite
    {
        protected override ITimeSeries TimeSeriesFactory() => new TimeSeries(Wrapper.Table);
    }
}
