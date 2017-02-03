using Cassandra.Data.Linq;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class CasTimeSeries : TimeSeries
    {
        public CasTimeSeries(Table<Event> table) : base(table) { }

        private TimeGuid lastId = TimeGuid.MinValue;

        public override Event Write(EventProto ev)
        {
            Event eventToWrite;

            do
            {
                eventToWrite = new Event(lastId, ev);
                lastId = new TimeGuid(lastId.GetTimestamp().AddMilliseconds(1), lastId.GetClockSequence(), lastId.GetNode());
            } while (!table.Insert(eventToWrite).IfNotExists().Execute().Applied);

            return eventToWrite;
        }
    }
}