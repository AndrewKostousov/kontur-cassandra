using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Series;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;
using JetBrains.Annotations;

namespace CassandraTimeSeries.Model
{
    public class CasTimeSeries : SimpleTimeSeries
    {
        private static readonly TimeUuid ClosingTimeUuid = TimeGuid.MaxValue.ToTimeUuid();

        private readonly CasSynchronizationHelper syncHelper;
        private readonly ISession session;

        public CasTimeSeries(Table<EventsCollection> eventsTable, Table<CasTimeSeriesSyncData> synchronizationTable, 
                             TimeLinePartitioner partitioner, uint operationalTimeoutMilliseconds=10000) 
            : base(eventsTable, partitioner, operationalTimeoutMilliseconds)
        {
            session = eventsTable.GetSession();
            syncHelper = new CasSynchronizationHelper(synchronizationTable, partitioner);
        }

        public override Timestamp[] Write(params EventProto[] events)
        {
            var sw = Stopwatch.StartNew();

            StatementExecutionResult statementExecutionResult = null;

            while (sw.ElapsedMilliseconds < OperationalTimeoutMilliseconds)
            {
                var eventsToWrite = PackIntoCollection(events, syncHelper.CreateSynchronizedId);

                try
                {
                    statementExecutionResult = CompareAndUpdate(eventsToWrite, statementExecutionResult);
                }
                catch (DriverException exception)
                {
                    Logger.Log(exception);
                    if (IsCriticalError(exception)) throw;
                    continue;
                }

                syncHelper.UpdateLastWrittenTimeGuid(statementExecutionResult, eventsToWrite);

                if (statementExecutionResult.State == ExecutionState.Success)
                    return eventsToWrite.Select(x => x.Timestamp).ToArray();
            }

            throw new OperationTimeoutException(OperationalTimeoutMilliseconds);
        }

        private StatementExecutionResult CompareAndUpdate(EventsCollection eventToWrite, [CanBeNull] StatementExecutionResult lastUpdateResult)
        {
            var lastWrittenPartitionId = syncHelper.IdOfLastWrittenPartition;
            syncHelper.UpdateIdOfLastWrittenPartition(eventToWrite.PartitionId);

            if (ShouldCloseOutdatedPartitions(eventToWrite.PartitionId, lastWrittenPartitionId, lastUpdateResult))
                CloseAllPartitionsBefore(eventToWrite.PartitionId);

            return WriteEventToCurrentPartition(eventToWrite, eventToWrite.PartitionId != lastWrittenPartitionId);
        }

        private bool ShouldCloseOutdatedPartitions(long partitionId, long lastWrittenPartitionId, [CanBeNull] StatementExecutionResult lastUpdateResult)
        {
            var partitionDelta = partitionId - lastWrittenPartitionId;
            var isWritingToEmptyPartition = partitionDelta != 0;
            var isJumpedOverExactlyOnePartition = partitionDelta <= Partitioner.PartitionDuration.Ticks;
            var lastPartitionIsNotAlreadyClosed = lastUpdateResult != null && lastUpdateResult.State != ExecutionState.PartitionClosed;

            return isWritingToEmptyPartition && isJumpedOverExactlyOnePartition && lastPartitionIsNotAlreadyClosed;
        }

        private StatementExecutionResult WriteEventToCurrentPartition(EventsCollection e, bool isWritingToEmptyPartition)
        {
            return ExecuteStatement(CreateWriteEventStatement(e, isWritingToEmptyPartition));
        }

        private IStatement CreateWriteEventStatement(EventsCollection e, bool isWritingToEmptyPartition)
        {
            var writeEventStatement = session.Prepare(
                $"UPDATE {eventsTable.Name} " +
                "SET user_ids = ?, payloads = ?, max_id_in_partition = ?, event_ids = ? " +
                "WHERE last_event_id = ? AND partition_id = ? " +
                (isWritingToEmptyPartition ? "IF max_id_in_partition = NULL" : "IF max_id_in_partition < ?")
            );

            return isWritingToEmptyPartition
                ? writeEventStatement.Bind(e.UserIds, e.Payloads, e.LastEventId, e.EventIds, e.LastEventId, e.PartitionId)
                : writeEventStatement.Bind(e.UserIds, e.Payloads, e.LastEventId, e.EventIds, e.LastEventId, e.PartitionId, e.LastEventId);
        }

        private void CloseAllPartitionsBefore(long exclusiveLastPartitionId)
        {
            var idOfPartitionToClose = exclusiveLastPartitionId - Partitioner.PartitionDuration.Ticks;
            var executionState = ExecutionState.Success;

            while (executionState != ExecutionState.PartitionClosed && idOfPartitionToClose >= syncHelper.PartitionIdOfStartOfTimes)
            {
                executionState = ExecuteStatement(CreateClosePartitionStatement(idOfPartitionToClose)).State;
                idOfPartitionToClose -= Partitioner.PartitionDuration.Ticks;
            }
        }

        private IStatement CreateClosePartitionStatement(long idOfPartitionToClose)
        {
            return session.Prepare(
                $"UPDATE {eventsTable.Name} " +
                "SET max_id_in_partition = ? WHERE partition_id = ? " +
                "IF max_id_in_partition != ?"
            ).Bind(ClosingTimeUuid, idOfPartitionToClose, ClosingTimeUuid);
        }

        private StatementExecutionResult ExecuteStatement(IStatement statement)
        {
            var statementExecutionResult = session.Execute(statement).GetRows().Single();
            var isUpdateApplied = statementExecutionResult.GetValue<bool>("[applied]");

            if (isUpdateApplied) return new StatementExecutionResult {State = ExecutionState.Success};

            var partitionMaxTimeUuid = statementExecutionResult.GetValue<TimeUuid>("max_id_in_partition");

            return new StatementExecutionResult
            {
                State = partitionMaxTimeUuid == ClosingTimeUuid ? ExecutionState.PartitionClosed : ExecutionState.OutdatedId,
                PartitionMaxGuid = partitionMaxTimeUuid.ToTimeGuid()
            };
        }
    }
}