using System;
using System.Diagnostics;
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

        private readonly CasSynchronizationHelper syncHelper;
        private readonly ISession session;

        public CasTimeSeries(Table<Event> eventTable, Table<CasTimeSeriesSyncData> synchronizationTable, uint operationsTimeoutMilliseconds=10000) 
            : base(eventTable, operationsTimeoutMilliseconds)
        {
            session = eventTable.GetSession();
            syncHelper = new CasSynchronizationHelper(new CasStartOfTimesHelper(synchronizationTable));
        }

        public override Timestamp Write(EventProto ev)
        {
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < OperationsTimeoutMilliseconds)
            {
                var eventToWrite = new Event(syncHelper.CreateSynchronizedId(), ev);
                StatementExecutionResult statementExecutionResult;

                try
                {
                    statementExecutionResult = CompareAndUpdate(eventToWrite);
                }
                catch (DriverException exception)
                {
                    Logger.Log(exception);
                    if (!ShouldRetryAfter(exception)) throw;
                    continue;
                }

                syncHelper.UpdateLastWrittenTimeGuid(statementExecutionResult, eventToWrite);

                if (statementExecutionResult.State == ExecutionState.Success)
                    return eventToWrite.Timestamp;
            }

            throw new OperationTimeoutException(OperationsTimeoutMilliseconds, ev);
        }

        private StatementExecutionResult CompareAndUpdate(Event eventToWrite)
        {
            var isWritingToEmptyPartition = eventToWrite.PartitionId != syncHelper.IdOfLastWrittenPartition;
            syncHelper.UpdateIdOfLastWrittenPartition(eventToWrite.PartitionId);

            if (isWritingToEmptyPartition)
                CloseAllPartitionsBefore(syncHelper.IdOfLastWrittenPartition);

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

        private void CloseAllPartitionsBefore(long exclusiveLastPartitionId)
        {
            var idOfPartitionToClose = exclusiveLastPartitionId - Event.PartitionDutation.Ticks;
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