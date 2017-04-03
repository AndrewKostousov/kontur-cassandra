using System;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class CasTimeSeriesTestSequential : CommonTimeSeriesTestSequential<CasTimeSeriesDatabaseController>
    {
        protected override ITimeSeries TimeSeriesFactory(CasTimeSeriesDatabaseController c) => new CasTimeSeries(c, new TimeLinePartitioner());
    }

    [TestFixture]
    public class CasTimeSeriesTestParallel : CommonTimeSeriesTestParallel<CasTimeSeriesDatabaseController>
    {
        protected override ITimeSeries TimeSeriesFactory(CasTimeSeriesDatabaseController c) => new CasTimeSeries(c, new TimeLinePartitioner());
    }

    [TestFixture]
    public class CasTimeSeriesTestWrite : CommonTimeSeriesTestWrite<CasTimeSeriesDatabaseController>
    {
        protected override ITimeSeries TimeSeriesFactory(CasTimeSeriesDatabaseController c) => new CasTimeSeries(c, new TimeLinePartitioner());
    }
}
