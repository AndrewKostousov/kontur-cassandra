using CassandraTimeSeries.Model;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    public abstract class CommonTimeSeriesTestWrite : TimeSeriesTestBase
    {
        [Test]
        public void Write_WriteOne()
        {
            Series.Write(new EventProto());
        }

        [Test]
        public void Write_WriteMany()
        {
            for (var i = 0; i < 3; ++i)
                Series.Write(new EventProto());
        }

        [Test]
        public void Write_BulkWrite()
        {
            Series.Write(new EventProto(), new EventProto(), new EventProto());
        }
    }
}
