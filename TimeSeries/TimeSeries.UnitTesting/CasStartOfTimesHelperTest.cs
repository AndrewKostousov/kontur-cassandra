using System;
using System.Linq;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using Commons.Logging;
using Commons.TimeBasedUuid;
using FluentAssertions;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class CasStartOfTimesHelperTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Logging.SetUp();

            var database = new CasTimeSeriesDatabaseController();
            database.SetUpSchema();
        }

        [Test]
        public void StartOfTimes_ShouldBeTheSame_InAllSessions()
        {
            var helpers = Enumerable.Range(0, 4)
                .Select(_ => new CasTimeSeriesDatabaseController().SyncTable)
                .Select(t => new CasStartOfTimesHelper(t, new TimeLinePartitioner(), CreateGuidGenerator()));

            helpers.Select(x => x.StartOfTimes).Distinct().Count().Should().Be(1);
        }

        private TimeGuidGenerator CreateGuidGenerator()
        {
            return new TimeGuidGenerator(new PreciseTimestampGenerator(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100)));
        }
    }
}
