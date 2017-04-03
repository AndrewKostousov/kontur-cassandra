using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class SimpleTimeSeriesTestSequential : CommonTimeSeriesTestSequential<SimpleTimeSeriesDatabaseController>
    {
        protected override ITimeSeries TimeSeriesFactory(SimpleTimeSeriesDatabaseController c) => new SimpleTimeSeries(c, new TimeLinePartitioner());
    }

    [TestFixture]
    public class SimpleTimeSeriesTestParallel : CommonTimeSeriesTestParallel<SimpleTimeSeriesDatabaseController>
    {
        protected override bool ShouldFailWithManyWriters => true;

        protected override ITimeSeries TimeSeriesFactory(SimpleTimeSeriesDatabaseController c) => new SimpleTimeSeries(c, new TimeLinePartitioner());
    }

    [TestFixture]
    public class SimpleTimeSeriesTestWrite : CommonTimeSeriesTestWrite<SimpleTimeSeriesDatabaseController>
    {
        protected override ITimeSeries TimeSeriesFactory(SimpleTimeSeriesDatabaseController c) => new SimpleTimeSeries(c, new TimeLinePartitioner());
    }
}
