using System;
using System.Linq;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Series;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class CasTimeSeries : SimpleTimeSeries
    {
        private static readonly TimeUuid ClosingTimeUuid = TimeGuid.MaxValue.ToTimeUuid();

        private readonly CasLastWrittenData lastWrittenData;
        private readonly CasTimeSeriesSyncHelper syncHelper;
        private readonly ISession session;

        private readonly uint writeAttemptsLimit;

        public CasTimeSeries(Table<Event> eventTable, Table<CasTimeSeriesSyncData> syncTable, uint writeAttemptsLimit=100) : base(eventTable)
        {
            session = eventTable.GetSession();
            lastWrittenData = new CasLastWrittenData();
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
                eventToWrite = new Event(lastWrittenData.CreateSynchronizedId(syncHelper), ev);

                try
                {
                    statementExecutionResult = CompareAndUpdate(eventToWrite);
                }
                catch (Exception exception)
                {
                    Logger.Log(exception);
                    return null;
                }

                lastWrittenData.UpdateLastWrittenTimeGuid(statementExecutionResult, eventToWrite);
                
                if (++writeAttemptsMade >= writeAttemptsLimit)
                {
                    Logger.Log(new WriteTimeoutException(writeAttemptsLimit));
                    return null;
                }

            } while (statementExecutionResult.State != ExecutionState.Success);

            lastWrittenData.UpdateLastWrittenTimeGuid(eventToWrite.TimeGuid);

            return eventToWrite.Timestamp;
        }

        private StatementExecutionResult CompareAndUpdate(Event eventToWrite)
        {
            var isWritingToEmptyPartition = eventToWrite.PartitionId != lastWrittenData.PartitionId;
            lastWrittenData.UpdateLastWrittenPartitionId(eventToWrite.PartitionId);

            if (isWritingToEmptyPartition)
                ClosePreviousPartitions(lastWrittenData.PartitionId);

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