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

        public override Event Write(EventProto ev)
        {
            Event eventToWrite;
            
            do
            {
                eventToWrite = new Event(TimeGuid.NowGuid(), ev);
            } while (!CompareAndUpdate(eventToWrite));

            return eventToWrite;
        }

        private bool CompareAndUpdate(Event eventToWrite)
        {
            var e = eventToWrite;
            var session = table.GetSession();

            var updateStatement = session
                .Prepare(
                    $"UPDATE {table.Name} " +
                    "SET user_id = ?, payload = ?, ticks = ?, max_ticks = ? " +
                    "WHERE event_id = ? AND slice_id = ? " +
                    (isFirstWrite ? "IF max_ticks = NULL" : "IF max_ticks <= ticks"))   // "max_ticks <= ticks" fails when inserting first row
                .Bind(e.UserId, e.Payload, e.Ticks, e.Ticks, e.Id, e.SliceId);

            isFirstWrite = false;

            return Execute(session, updateStatement);
        }

        private bool Execute(ISession session, IStatement statement)
        {
            return session.Execute(statement).GetRows().Select(x => x.GetValue<bool>("[applied]")).Single();
        }
    }
}