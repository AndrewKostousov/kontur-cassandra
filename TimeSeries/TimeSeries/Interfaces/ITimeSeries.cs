using System.Collections.Generic;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Interfaces
{
    public interface ITimeSeries
    {
        TimeLinePartitioner Partitioner { get; }

        Timestamp[] Write(params EventProto[] events);
        void WriteWithoutSync(Event ev);
        Event[] ReadRange(Timestamp startExclusive, Timestamp endInclusive, int count=1000);
        Event[] ReadRange(TimeGuid startExclusive, TimeGuid endInclusive, int count=1000);
    }
}
