using System;
using System.Linq;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.Logging;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class CasTimeSeries : SimpleTimeSeries
    {
        private class StatementExecutionResult
        {
            public ExecutionState State { get; set; }
            public TimeGuid PartitionMaxGuid { get; set; }
        }

        private enum ExecutionState
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

        private readonly uint writeAttemptsLimit;

        public CasTimeSeries(Table<Event> eventTable, Table<CasTimeSeriesSyncData> syncTable, uint writeAttemptsLimit=100) : base(eventTable)
        {
            session = eventTable.GetSession();
            syncHelper = new CasTimeSeriesSyncHelper(syncTable);
            this.writeAttemptsLimit = writeAttemptsLimit;
        }

        public override Timestamp Write(EventProto ev)
        {
            Event eventToWrite;
            StatementExecutionResult statementExecutionResult;

            var writeAttemptsMade = 0;

            do
            {
                eventToWrite = new Event(CreateSynchronizedId(), ev);

                try
                {
                    statementExecutionResult = CompareAndUpdate(eventToWrite);
                }
                catch (Exception)
                {
                    throw new ApplicationException("Cannot write event: please, check database connection.");
                }

                if (statementExecutionResult.State == ExecutionState.PartitionClosed)
                    lastWrittenTimeGuid = TimeGuid.MinForTimestamp(new Timestamp(eventToWrite.PartitionId) + Event.PartitionDutation);

                if (statementExecutionResult.State == ExecutionState.OutdatedId)
                    lastWrittenTimeGuid = statementExecutionResult.PartitionMaxGuid;

                if (++writeAttemptsMade >= writeAttemptsLimit)
                    throw new ApplicationException("Cannot write event: limit is exceeded for write attempts.");

            } while (statementExecutionResult.State != ExecutionState.Success);

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

        private StatementExecutionResult CompareAndUpdate(Event eventToWrite)
        {
            var isWritingToEmptyPartition = eventToWrite.PartitionId != lastWrittenPartitionId;
            lastWrittenPartitionId = eventToWrite.PartitionId;

            if (isWritingToEmptyPartition)
                ClosePreviousPartitions(lastWrittenPartitionId);

            return WriteEventToCurrentPartition(eventToWrite, isWritingToEmptyPartition);
        }

        private StatementExecutionResult WriteEventToCurrentPartition(Event e, bool isWritingToEmptyPartition)
        {
            return ExecuteStatement(CreateWriteEventStatement(e, isWritingToEmptyPartition));
        }

        private IStatement CreateWriteEventStatement(Event e, bool isWritingToEmptyPartition)
        {
            var writeEventStatement = session.Prepare(
                $"UPDATE {eventTable.Name} " +
                "SET user_id = ?, payload = ?, max_id = ? " +
                "WHERE event_id = ? AND partition_id = ? " +
                (isWritingToEmptyPartition ? "IF max_id = NULL" : "IF max_id < ?")
            );

            return isWritingToEmptyPartition
                ? writeEventStatement.Bind(e.UserId, e.Payload, e.Id, e.Id, e.PartitionId)
                : writeEventStatement.Bind(e.UserId, e.Payload, e.Id, e.Id, e.PartitionId, e.Id);
        }

        private void ClosePreviousPartitions(long currentPartitionId)
        {
            var idOfPartitionToClose = currentPartitionId - Event.PartitionDutation.Ticks;
            var executionState = ExecutionState.Success;

            while (executionState != ExecutionState.PartitionClosed && idOfPartitionToClose >= syncHelper.PartitionIdOfStartOfTimes)
            {
                executionState = ExecuteStatement(CreateClosePartitionStatement(idOfPartitionToClose)).State;
                idOfPartitionToClose -= Event.PartitionDutation.Ticks;
            }
        }

        private IStatement CreateClosePartitionStatement(long idOfPartitionToClose)
        {
            return session.Prepare(
                $"UPDATE {eventTable.Name} " +
                "SET max_id = ? WHERE partition_id = ? " +
                "IF max_id != ?"
            ).Bind(ClosingTimeUuid, idOfPartitionToClose, ClosingTimeUuid);
        }

        private StatementExecutionResult ExecuteStatement(IStatement statement)
        {
            var statementExecutionResult = session.Execute(statement).GetRows().Single();
            var isUpdateApplied = statementExecutionResult.GetValue<bool>("[applied]");

            if (isUpdateApplied) return new StatementExecutionResult {State = ExecutionState.Success};

            var partitionMaxTimeUuid = statementExecutionResult.GetValue<TimeUuid>("max_id");

            return new StatementExecutionResult
            {
                State = partitionMaxTimeUuid == ClosingTimeUuid ? ExecutionState.PartitionClosed : ExecutionState.OutdatedId,
                PartitionMaxGuid = partitionMaxTimeUuid.ToTimeGuid()
            };
        }
    }
}