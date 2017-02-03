using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class CasTimeSeriesTestSequential : CommonTimeSeriesTestSequential
    {
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(Wrapper.Table);
    }

    [TestFixture]
    public class CasTimeSeriesTestParallel : CommonTimeSeriesTestParallel
    {
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(Wrapper.Table);
    }

    [TestFixture]
    public class CasTimeSeriesTestWrite : CommonTimeSeriesTestWrite
    {
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(Wrapper.Table);
    }
}
