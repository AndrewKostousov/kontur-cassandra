using System.Linq;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using Commons.TimeBasedUuid;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    public abstract class CommonTimeSeriesTestWrite<TDatabaseController> : TimeSeriesTestBase<TDatabaseController>
        where TDatabaseController : IDatabaseController, new()
    {
        [Test]
        public void Write_SingleWrite()
        {
            Series.Write(new EventProto());
        }

        [Test]
        public void Write_SequentialWrite()
        {
            for (var i = 0; i < 3; ++i)
                Series.Write(new EventProto());
        }

        [Test]
        public void Write_BulkWrite()
        {
            Series.Write(new EventProto(), new EventProto(), new EventProto());
        }

        [Test]
        public void Write_CanReadAfterWrite()
        {
            var controller = new TDatabaseController();
            controller.SetUpSchema();

            var series = TimeSeriesFactory(controller);
            var ev = new EventProto();

            series.Write(ev);
            series.ReadRange((TimeGuid) null, null).Select(e => e.Proto).ShouldBeExactly(ev);
        }
    }
}
