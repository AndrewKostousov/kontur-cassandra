using System.Linq;
using Cassandra;
using Cassandra.Data.Linq;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class CasTimeSeries : TimeSeries
    {
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

            lastSliceId = eventToWrite.SliceId;

            return eventToWrite;
        }

        private Event CreateEventToWrite(EventProto ev)
        {
            var eventToWrite = new Event(TimeGuid.NowGuid(), ev);
            

            return eventToWrite;
        }

        private bool CompareAndUpdate(Event eventToWrite)
        {
            var isFirstWrite = eventToWrite.SliceId != lastSliceId;

            return isFirstWrite 
                ? WriteFirstEvent(eventToWrite) 
                : WriteNextEvent(eventToWrite);

            // FIXME: иногда теряется последний эвент в слайсе, так как колонка max_id общая только
            // для записей в одном слайсе. Если появляется эвент в следующем слайсе, то ничего
            // не мешает вставить следующий эвент в предыдущий слайс. 

            //var preparedStatement = session.Prepare(
            //    $"UPDATE {table.Name} " +
            //    "SET user_id = ?, payload = ?, max_id = ? " +
            //    "WHERE event_id = ? AND slice_id = ? " +
            //    (isFirstWrite ? "IF max_id = NULL" : "IF max_id < ?"));   // "max_ticks < ticks" fails when inserting first row

            //var updatePreviousPartition = session.Prepare(
            //    $"UPDATE {table.Name} " +
            //    "SET max_id = ? WHERE slice_id = ? " +
            //    "IF max_id < ?"
            //);

            //var batch = new BatchStatement();

            //batch.Add(isFirstWrite
            //    ? preparedStatement.Bind(e.UserId, e.Payload, e.Id, e.Id, e.SliceId)
            //    : preparedStatement.Bind(e.UserId, e.Payload, e.Id, e.Id, e.SliceId, e.Id));

            //batch.Add(updatePreviousPartition.Bind(e.Id, lastSliceId, e.Id));

            //return Execute(batch);
        }

        private bool WriteFirstEvent(Event eventToWrite)
        {
            var e = eventToWrite;

            var prevSliceId = e.SliceId - Event.SliceDutation.Ticks;

            var currentSliceUpdateStatement = session.Prepare(
                $"UPDATE {table.Name} " +
                "SET user_id = ?, payload = ?, max_id = ? " +
                "WHERE event_id = ? AND slice_id = ? " +
                "IF max_id = NULL"
            ).Bind(e.UserId, e.Payload, e.Id, e.Id, e.SliceId);

            var prevSliceUpdateStatement = session.Prepare(
                $"UPDATE {table.Name} " +
                "SET max_id = ? WHERE slice_id = ? " +
                "IF max_id < ?"
            ).Bind(e.Id, prevSliceId, e.Id);

            //var batch = new BatchStatement()
            //    .Add(currentSliceUpdateStatement)
            //    .Add(prevSliceUpdateStatement);

            Execute(prevSliceUpdateStatement);

            return Execute(currentSliceUpdateStatement);
        }

        private bool WriteNextEvent(Event eventToWrite)
        {
            var e = eventToWrite;

            var updateStatement = session.Prepare(
                $"UPDATE {table.Name} " +
                "SET user_id = ?, payload = ?, max_id = ? " +
                "WHERE event_id = ? AND slice_id = ? " +
                "IF max_id < ?"
            ).Bind(e.UserId, e.Payload, e.Id, e.Id, e.SliceId, e.Id);

            return Execute(updateStatement);
        }

        private bool Execute(IStatement statement)
        {
            return session.Execute(statement).GetRows().Select(x => x.GetValue<bool>("[applied]")).Single();
        }
    }
}