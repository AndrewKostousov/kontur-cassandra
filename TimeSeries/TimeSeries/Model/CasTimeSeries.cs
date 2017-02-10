using System.Linq;
using Cassandra;
using Cassandra.Data.Linq;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class CasTimeSeries : TimeSeries
    {
        private bool isFirstWrite = true;
        private long lastSliceId;

        private readonly ISession session;

        public CasTimeSeries(Table<Event> table) : base(table)
        {
            session = table.GetSession();
        }

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

            // FIXME: иногда теряется последний эвент в слайсе, так как колонка max_id общая только
            // для записей в одном слайсе. Если появляется эвент в следующем слайсе, то ничего
            // не мешает вставить следующий эвент в предыдущий слайс. 

            var preparedStatement = session.Prepare(
                $"UPDATE {table.Name} " +
                "SET user_id = ?, payload = ?, max_id = ? " +
                "WHERE event_id = ? AND slice_id = ? " +
                (isFirstWrite ? "IF max_id = NULL" : "IF max_id < ?"));   // "max_ticks < ticks" fails when inserting first row

            return Execute(isFirstWrite 
                ? preparedStatement.Bind(e.UserId, e.Payload, e.Id, e.Id, e.SliceId) 
                : preparedStatement.Bind(e.UserId, e.Payload, e.Id, e.Id, e.SliceId, e.Id));
        }

        private bool Execute(IStatement statement)
        {
            return session.Execute(statement).GetRows().Select(x => x.GetValue<bool>("[applied]")).Single();
        }
    }
}