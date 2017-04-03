using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class EdiSeriesTestSequential : CommonTimeSeriesTestSequential<EdiTimeSeriesDatabaseController>
    {
        protected override ITimeSeries TimeSeriesFactory(EdiTimeSeriesDatabaseController c) => new EdiTimeSeriesWrapper(c, new TimeLinePartitioner());
    }

    [TestFixture]
    public class EdiSeriesTestParallel : CommonTimeSeriesTestParallel<EdiTimeSeriesDatabaseController>
    {
        protected override ITimeSeries TimeSeriesFactory(EdiTimeSeriesDatabaseController c) => new EdiTimeSeriesWrapper(c, new TimeLinePartitioner());
    }

    [TestFixture]
    public class EdiSeriesTestWrite : CommonTimeSeriesTestWrite<EdiTimeSeriesDatabaseController>
    {
        protected override ITimeSeries TimeSeriesFactory(EdiTimeSeriesDatabaseController c) => new EdiTimeSeriesWrapper(c, new TimeLinePartitioner());
    }
}
