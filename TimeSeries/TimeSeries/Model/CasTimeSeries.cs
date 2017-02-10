using System.Linq;
using Cassandra;
using Cassandra.Data.Linq;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class CasTimeSeries : TimeSeries
    {
        public CasTimeSeries(Table<Event> table) : base(table) { }

        private bool isFirstWrite = true;
        private long lastSliceId;

        public override Event Write(EventProto ev)
        {
            Event eventToWrite;
            
            do
            {
                eventToWrite = CreateEventToWrite(ev);
            } while (!CompareAndUpdate(eventToWrite));

            return eventToWrite;
        }

        private Event CreateEventToWrite(EventProto ev)
        {
            var eventToWrite = new Event(TimeGuid.NowGuid(), ev);

            isFirstWrite = eventToWrite.SliceId != lastSliceId;
            lastSliceId = eventToWrite.SliceId;

            return eventToWrite;
        }

        private bool CompareAndUpdate(Event eventToWrite)
        {
            var e = eventToWrite;
            var session = table.GetSession();

            var preparedStatement = session.Prepare(
                $"UPDATE {table.Name} " +
                "SET user_id = ?, payload = ?, max_id = ? " +
                "WHERE event_id = ? AND slice_id = ? " +
                (isFirstWrite ? "IF max_id = NULL" : "IF max_id <= ?"));   // "max_ticks <= ticks" fails when inserting first row

            return Execute(session, isFirstWrite 
                ? preparedStatement.Bind(e.UserId, e.Payload, e.Id, e.Id, e.SliceId) 
                : preparedStatement.Bind(e.UserId, e.Payload, e.Id, e.Id, e.SliceId, e.Id));
        }

        private bool Execute(ISession session, IStatement statement)
        {
            return session.Execute(statement).GetRows().Select(x => x.GetValue<bool>("[applied]")).Single();
        }
    }
}