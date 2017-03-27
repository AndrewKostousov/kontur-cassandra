using System.Diagnostics;
using System.Linq;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.Logging;
using Commons.TimeBasedUuid;
using JetBrains.Annotations;

namespace CassandraTimeSeries.Model
{
    public class CasTimeSeries : SimpleTimeSeries
    {
        private static readonly TimeUuid ClosingTimeUuid = TimeGuid.MaxValue.ToTimeUuid();

        private long idOfLastWrittenPartition;
        private TimeGuid lastWrittenTimeGuid;
        private readonly CasStartOfTimesHelper startOfTimesHelper;

        private readonly ISession session;

        public CasTimeSeries(Table<EventsCollection> eventsTable, Table<CasTimeSeriesSyncData> synchronizationTable, 
                             TimeLinePartitioner partitioner, uint operationalTimeoutMilliseconds=10000) 
            : base(eventsTable, partitioner, operationalTimeoutMilliseconds)
        {
            session = eventsTable.GetSession();
            startOfTimesHelper = new CasStartOfTimesHelper(synchronizationTable, partitioner);
        }

        public override Timestamp[] Write(params EventProto[] events)
        {
            var sw = Stopwatch.StartNew();

            StatementExecutionResult statementExecutionResult = null;

            while (sw.ElapsedMilliseconds < OperationalTimeoutMilliseconds)
            {
                var eventsToWrite = PackIntoCollection(events);

                try
                {
                    statementExecutionResult = CompareAndUpdate(eventsToWrite, statementExecutionResult);
                }
                catch (DriverException exception)
                {
                    Log.For(this).Error(exception, "Cassandra driver exception occured during write.");
                    if (exception.IsCritical()) throw;
                    continue;
                }

                UpdateLastWrittenTimeGuid(statementExecutionResult, eventsToWrite);

                if (statementExecutionResult.State == ExecutionState.Success)
                    return eventsToWrite.Select(_ => eventsToWrite.TimeGuid.GetTimestamp()).ToArray();
            }

            throw new OperationTimeoutException(OperationalTimeoutMilliseconds);
        }

        private EventsCollection PackIntoCollection(EventProto[] eventProtos)
        {
            var id = CreateSynchronizedId();
            var partitionId = Partitioner.CreatePartitionId(id.GetTimestamp());

            return new EventsCollection(id, partitionId, eventProtos);
        }

        private void UpdateLastWrittenTimeGuid(StatementExecutionResult compareAndUpdateResult, EventsCollection eventToWrite)
        {
            if (compareAndUpdateResult.State == ExecutionState.PartitionClosed)
                lastWrittenTimeGuid = TimeGuid.MinForTimestamp(new Timestamp(eventToWrite.PartitionId) + Partitioner.PartitionDuration);

            if (compareAndUpdateResult.State == ExecutionState.OutdatedId)
                lastWrittenTimeGuid = compareAndUpdateResult.PartitionMaxGuid;

            if (compareAndUpdateResult.State == ExecutionState.Success)
                lastWrittenTimeGuid = eventToWrite.TimeUuid.ToTimeGuid();
        }

        private TimeGuid CreateSynchronizedId()
        {
            var nowGuid = TimeGuid.NowGuid();

            if (lastWrittenTimeGuid != null && lastWrittenTimeGuid.GetTimestamp() >= nowGuid.GetTimestamp())
                return lastWrittenTimeGuid.AddMilliseconds(1);

            if (startOfTimesHelper.StartOfTimes.GetTimestamp() >= nowGuid.GetTimestamp())
                return startOfTimesHelper.StartOfTimes.AddMilliseconds(1);

            return nowGuid;
        }

        private StatementExecutionResult CompareAndUpdate(EventsCollection eventToWrite, [CanBeNull] StatementExecutionResult lastUpdateResult)
        {
            var lastWrittenPartitionId = idOfLastWrittenPartition;
            idOfLastWrittenPartition = eventToWrite.PartitionId;

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
                "SET user_ids = ?, payloads = ?, max_id_in_partition = ?" +
                "WHERE time_uuid = ? AND partition_id = ? " +
                (isWritingToEmptyPartition ? "IF max_id_in_partition = NULL" : "IF max_id_in_partition < ?")
            );

            return isWritingToEmptyPartition
                ? writeEventStatement.Bind(e.UserIds, e.Payloads, e.TimeUuid, e.TimeUuid, e.PartitionId)
                : writeEventStatement.Bind(e.UserIds, e.Payloads, e.TimeUuid, e.TimeUuid, e.PartitionId, e.TimeUuid);
        }

        private void CloseAllPartitionsBefore(long exclusiveLastPartitionId)
        {
            var idOfPartitionToClose = exclusiveLastPartitionId - Partitioner.PartitionDuration.Ticks;
            var executionState = ExecutionState.Success;

            while (executionState != ExecutionState.PartitionClosed && idOfPartitionToClose >= startOfTimesHelper.PartitionIdOfStartOfTimes)
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