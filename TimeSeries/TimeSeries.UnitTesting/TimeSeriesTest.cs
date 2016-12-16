using CassandraTimeSeries.Model;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class TimeSeriesTestSequential : CommonTimeSeriesTestSequential
    {
        protected override TimeSeries TimeSeriesFactory() => new TimeSeries(Wrapper.Table);
    }

    [TestFixture]
    public class TimeSeriesTestParallel : CommonTimeSeriesTestParallel
    {
        protected override TimeSeries TimeSeriesFactory() => new TimeSeries(Wrapper.Table);
    }
}
