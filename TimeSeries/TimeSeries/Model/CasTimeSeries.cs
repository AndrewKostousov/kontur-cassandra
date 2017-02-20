using System.Collections.Generic;
using System.Linq;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class CasTimeSeries : SimpleTimeSeries
    {
        class UpdateResult
        {
            public UpdateState State { get; set; }
            public TimeGuid PartitionMaxGuid { get; set; }
        }

        enum UpdateState
        {
            Success,
            OutdatedId,
            PartitionClosed,
        }

        private readonly CasTimeSeriesSyncHelper syncHelper;
        private readonly ISession session;

        private long lastWrittenPartitionId;
        private TimeGuid lastWrittenTimeGuid;

        public CasTimeSeries(Table<Event> eventTable, Table<CasTimeSeriesSyncColumn> syncTable) : base(eventTable)
        {
            session = eventTable.GetSession();
            syncHelper = new CasTimeSeriesSyncHelper(syncTable);
        }

        public override Timestamp Write(EventProto ev)
        {
            Event eventToWrite;
            UpdateResult updateResult;

            do
            {
                var guid = CreateSyncId();

                updateResult = CompareAndUpdate(eventToWrite = new Event(guid, ev));

                if (updateResult.State == UpdateState.PartitionClosed)
                    lastWrittenTimeGuid = TimeGuid.MinForTimestamp(new Timestamp(eventToWrite.PartitionId) + Event.PartitionDutation);

                if (updateResult.State == UpdateState.OutdatedId)
                    lastWrittenTimeGuid = updateResult.PartitionMaxGuid;

            } while (updateResult.State != UpdateState.Success);

            lastWrittenTimeGuid = eventToWrite.TimeGuid;

            return eventToWrite.Timestamp;
        }

        private TimeGuid CreateSyncId()
        {
            var nowGuid = TimeGuid.NowGuid();

            if (lastWrittenTimeGuid != null && lastWrittenTimeGuid.GetTimestamp() >= nowGuid.GetTimestamp())
                return lastWrittenTimeGuid.Increment();

            if (syncHelper.StartOfTimes.GetTimestamp() >= nowGuid.GetTimestamp())
                return syncHelper.StartOfTimes.Increment();

            return nowGuid;
        }

        private UpdateResult CompareAndUpdate(Event eventToWrite)
        {
            var isWritePartitionEmpty = eventToWrite.PartitionId != lastWrittenPartitionId;
            lastWrittenPartitionId = eventToWrite.PartitionId;

            if (isWritePartitionEmpty)
                ClosePreviousPartitions(lastWrittenPartitionId);

            return WriteEventToCurrentPartition(eventToWrite, isWritePartitionEmpty);
        }

        private UpdateResult WriteEventToCurrentPartition(Event e, bool isWritePartitionEmpty)
        {
            var updateStatement = session.Prepare(
                $"UPDATE {eventTable.Name} " +
                "SET user_id = ?, payload = ?, max_id = ? " +
                "WHERE event_id = ? AND partition_id = ? " +
                (isWritePartitionEmpty ? "IF max_id = NULL" : "IF max_id < ?")
            );

            return ExecuteUpdate(isWritePartitionEmpty
                ? updateStatement.Bind(e.UserId, e.Payload, e.Id, e.Id, e.PartitionId)
                : updateStatement.Bind(e.UserId, e.Payload, e.Id, e.Id, e.PartitionId, e.Id));
        }

        private void ClosePreviousPartitions(long currentPartitionId)
        {
            var partitionIdToClose = currentPartitionId - Event.PartitionDutation.Ticks;
            UpdateState updateState = UpdateState.Success;

            var maxUuid = TimeGuid.MaxValue.ToTimeUuid();

            while (updateState != UpdateState.PartitionClosed && partitionIdToClose >= syncHelper.PartitionIdOfStartOfTimes)
            {
                var updateStatement = session.Prepare(
                    $"UPDATE {eventTable.Name} " +
                    "SET max_id = ? WHERE partition_id = ? " +
                    "IF max_id != ?"
                ).Bind(maxUuid, partitionIdToClose, maxUuid);

                updateState = ExecuteUpdate(updateStatement).State;
                partitionIdToClose -= Event.PartitionDutation.Ticks;
            }
        }

        private UpdateResult ExecuteUpdate(IStatement statement)
        {
            var execResult = session.Execute(statement).GetRows().Single();
            var isApplied = execResult.GetValue<bool>("[applied]");

            if (isApplied) return new UpdateResult {State = UpdateState.Success};

            var partitionMaxGuid = execResult.GetValue<TimeUuid>("max_id").ToTimeGuid();

            return new UpdateResult
            {
                State = partitionMaxGuid == TimeGuid.MaxValue ? UpdateState.PartitionClosed : UpdateState.OutdatedId,
                PartitionMaxGuid = partitionMaxGuid
            };
        }
    }
}