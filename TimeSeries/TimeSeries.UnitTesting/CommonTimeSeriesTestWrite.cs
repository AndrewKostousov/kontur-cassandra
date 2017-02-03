using CassandraTimeSeries.Model;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    public abstract class CommonTimeSeriesTestWrite : TimeSeriesTestBase
    {
        [Test]
        public void Write_GenericCase()
        {
            Series.Write(new EventProto());
        } 
    }
}
