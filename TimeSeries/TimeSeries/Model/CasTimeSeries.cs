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
        class WriteExecutionResult
        {
            public WriteExecutionState State { get; set; }
            public TimeGuid PartitionMaxGuid { get; set; }
        }

        enum WriteExecutionState
        {
            Success,
            OutdatedId,
            PartitionClosed,
        }

        private static readonly TimeUuid ClosingTimeUuid = TimeGuid.MaxValue.ToTimeUuid();

        private readonly CasTimeSeriesSyncHelper syncHelper;
        private readonly ISession session;

        private long lastWrittenPartitionId;
        private TimeGuid lastWrittenTimeGuid;

        private readonly int writeAttemptsLimit;

        public CasTimeSeries(Table<Event> eventTable, Table<CasTimeSeriesSyncData> syncTable, int writeAttemptsLimit=100) : base(eventTable)
        {
            session = eventTable.GetSession();
            syncHelper = new CasTimeSeriesSyncHelper(syncTable);
            this.writeAttemptsLimit = writeAttemptsLimit;
        }

        public override Timestamp Write(EventProto ev)
        {
            Event eventToWrite;
            WriteExecutionResult writeExecutionResult;

            var writeAttemptsMade = 0;

            do
            {
                writeExecutionResult = CompareAndUpdate(eventToWrite = new Event(CreateSynchronizedId(), ev));

                if (writeExecutionResult.State == WriteExecutionState.PartitionClosed)
                    lastWrittenTimeGuid = TimeGuid.MinForTimestamp(new Timestamp(eventToWrite.PartitionId) + Event.PartitionDutation);

                if (writeExecutionResult.State == WriteExecutionState.OutdatedId)
                    lastWrittenTimeGuid = writeExecutionResult.PartitionMaxGuid;

                if (++writeAttemptsMade >= writeAttemptsLimit) throw new WriteTimeoutException(writeAttemptsLimit);

            } while (writeExecutionResult.State != WriteExecutionState.Success);

            lastWrittenTimeGuid = eventToWrite.TimeGuid;

            return eventToWrite.Timestamp;
        }

        private TimeGuid CreateSynchronizedId()
        {
            var nowGuid = TimeGuid.NowGuid();

            if (lastWrittenTimeGuid != null && lastWrittenTimeGuid.GetTimestamp() >= nowGuid.GetTimestamp())
                return lastWrittenTimeGuid.Increment();

            if (syncHelper.StartOfTimes.GetTimestamp() >= nowGuid.GetTimestamp())
                return syncHelper.StartOfTimes.Increment();

            return nowGuid;
        }

        private WriteExecutionResult CompareAndUpdate(Event eventToWrite)
        {
            var isWritePartitionEmpty = eventToWrite.PartitionId != lastWrittenPartitionId;
            lastWrittenPartitionId = eventToWrite.PartitionId;

            if (isWritePartitionEmpty)
                ClosePreviousPartitions(lastWrittenPartitionId);

            return WriteEventToCurrentPartition(eventToWrite, isWritePartitionEmpty);
        }

        private WriteExecutionResult WriteEventToCurrentPartition(Event e, bool isWritePartitionEmpty)
        {
            var updateStatement = session.Prepare(
                $"UPDATE {eventTable.Name} " +
                "SET user_id = ?, payload = ?, max_id = ? " +
                "WHERE event_id = ? AND partition_id = ? " +
                (isWritePartitionEmpty ? "IF max_id = NULL" : "IF max_id < ?")
            );

            return ExecuteUpdateStatement(isWritePartitionEmpty
                ? updateStatement.Bind(e.UserId, e.Payload, e.Id, e.Id, e.PartitionId)
                : updateStatement.Bind(e.UserId, e.Payload, e.Id, e.Id, e.PartitionId, e.Id));
        }

        private void ClosePreviousPartitions(long currentPartitionId)
        {
            var partitionIdToClose = currentPartitionId - Event.PartitionDutation.Ticks;
            WriteExecutionState writeExecutionState = WriteExecutionState.Success;

            while (writeExecutionState != WriteExecutionState.PartitionClosed && partitionIdToClose >= syncHelper.PartitionIdOfStartOfTimes)
            {
                var updateStatement = session.Prepare(
                    $"UPDATE {eventTable.Name} " +
                    "SET max_id = ? WHERE partition_id = ? " +
                    "IF max_id != ?"
                ).Bind(ClosingTimeUuid, partitionIdToClose, ClosingTimeUuid);

                writeExecutionState = ExecuteUpdateStatement(updateStatement).State;
                partitionIdToClose -= Event.PartitionDutation.Ticks;
            }
        }

        private WriteExecutionResult ExecuteUpdateStatement(IStatement statement)
        {
            var execResult = session.Execute(statement).GetRows().Single();
            var isApplied = execResult.GetValue<bool>("[applied]");

            if (isApplied) return new WriteExecutionResult {State = WriteExecutionState.Success};

            var partitionMaxTimeUuid = execResult.GetValue<TimeUuid>("max_id");

            return new WriteExecutionResult
            {
                State = partitionMaxTimeUuid == ClosingTimeUuid ? WriteExecutionState.PartitionClosed : WriteExecutionState.OutdatedId,
                PartitionMaxGuid = partitionMaxTimeUuid.ToTimeGuid()
            };
        }
    }
}