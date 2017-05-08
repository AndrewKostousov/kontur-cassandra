using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Series;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.Logging;
using Commons.TimeBasedUuid;
using JetBrains.Annotations;

namespace CassandraTimeSeries.Model
{
    public class CasTimeSeries : BaseTimeSeries
    {
        private static readonly TimeUuid ClosingTimeUuid = TimeGuid.MaxValue.ToTimeUuid();

        private long idOfLastWrittenPartition;
        private TimeGuid lastWrittenTimeGuid;

        private readonly CasStartOfTimesHelper startOfTimesHelper;

        public CasTimeSeries(CasTimeSeriesDatabaseController databaseController, TimeLinePartitioner partitioner, uint operationalTimeoutMilliseconds = 10000)
            : base(databaseController.EventsTable, partitioner, operationalTimeoutMilliseconds)
        {
            startOfTimesHelper = new CasStartOfTimesHelper(databaseController.SyncTable, partitioner, TimeGuidGenerator);
        }

        public override void WriteWithoutSync(Event ev)
        {
            var eventsCollection = new EventsCollection(ev.Id, Partitioner.CreatePartitionId(ev.TimeGuid.GetTimestamp()), ev.Proto);

            EventsTable.Insert(eventsCollection).Execute();

            CloseAllPartitionsBefore(Partitioner.Increment(eventsCollection.PartitionId));
        }

        public override Timestamp[] Write(params EventProto[] events)
        {
            var sw = Stopwatch.StartNew();
            StatementExecutionResult statementExecutionResult = null;

            if (lastWrittenTimeGuid == null)
                lastWrittenTimeGuid = ReadLastWrittenTimeGuid();

            while (sw.ElapsedMilliseconds < OperationalTimeoutMilliseconds)
            {
                var eventsToWrite = PackIntoCollection(events, CreateSynchronizedId());

                try
                {
                    statementExecutionResult = CompareAndUpdate(eventsToWrite, statementExecutionResult);
                }
                catch (Exception exception)
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

        private void UpdateLastWrittenTimeGuid(StatementExecutionResult compareAndUpdateResult,
            EventsCollection eventToWrite)
        {
            if (compareAndUpdateResult.State == ExecutionState.PartitionClosed)
                lastWrittenTimeGuid =
                    TimeGuid.MinForTimestamp(new Timestamp(eventToWrite.PartitionId) + Partitioner.PartitionDuration);

            if (compareAndUpdateResult.State == ExecutionState.OutdatedId)
                lastWrittenTimeGuid = compareAndUpdateResult.PartitionMaxGuid;

            if (compareAndUpdateResult.State == ExecutionState.Success)
                lastWrittenTimeGuid = eventToWrite.TimeUuid.ToTimeGuid();
        }

        private TimeGuid CreateSynchronizedId()
        {
            var nowGuid = NewTimeGuid();

            if (lastWrittenTimeGuid != null && lastWrittenTimeGuid.GetTimestamp() >= nowGuid.GetTimestamp())
                return lastWrittenTimeGuid.AddMilliseconds(1);

            if (startOfTimesHelper.StartOfTimes.GetTimestamp() >= nowGuid.GetTimestamp())
                return startOfTimesHelper.StartOfTimes.AddMilliseconds(1);

            return nowGuid;
        }

        private StatementExecutionResult CompareAndUpdate(EventsCollection eventToWrite,
            [CanBeNull] StatementExecutionResult lastUpdateResult)
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
            var lastPartitionIsNotAlreadyClosed = lastUpdateResult == null || lastUpdateResult.State != ExecutionState.PartitionClosed;

            return isWritingToEmptyPartition && lastPartitionIsNotAlreadyClosed;
        }

        private StatementExecutionResult WriteEventToCurrentPartition(EventsCollection e, bool isWritingToEmptyPartition)
        {
            return ExecuteStatement(CreateWriteEventStatement(e, isWritingToEmptyPartition));
        }

        private IStatement CreateWriteEventStatement(EventsCollection e, bool isWritingToEmptyPartition)
        {
            var writeEventStatement = Session.Prepare(
                $"UPDATE {EventsTable.Name} " +
                "SET user_ids = ?, payloads = ?, max_id_in_partition = ?" +
                "WHERE time_uuid = ? AND partition_id = ? " +
                (isWritingToEmptyPartition ? "IF max_id_in_partition = NULL" : "IF max_id_in_partition = ?")
            );

            return isWritingToEmptyPartition
                ? writeEventStatement.Bind(e.UserIds, e.Payloads, e.TimeUuid, e.TimeUuid, e.PartitionId)
                : writeEventStatement.Bind(e.UserIds, e.Payloads, e.TimeUuid, e.TimeUuid, e.PartitionId, lastWrittenTimeGuid.ToTimeUuid());
        }

        private void CloseAllPartitionsBefore(long exclusiveLastPartitionId)
        {
            var idOfPartitionToClose = exclusiveLastPartitionId - Partitioner.PartitionDuration.Ticks;
            var executionState = ExecutionState.Success;

            while (executionState != ExecutionState.PartitionClosed &&
                   idOfPartitionToClose >= startOfTimesHelper.PartitionIdOfStartOfTimes)
            {
                executionState = ExecuteStatement(CreateClosePartitionStatement(idOfPartitionToClose)).State;
                idOfPartitionToClose = Partitioner.Decrement(idOfPartitionToClose);
            }
        }

        private IStatement CreateClosePartitionStatement(long idOfPartitionToClose)
        {
            return Session.Prepare(
                $"UPDATE {EventsTable.Name} " +
                "SET max_id_in_partition = ? WHERE partition_id = ? " +
                "IF max_id_in_partition != ?"
            ).Bind(ClosingTimeUuid, idOfPartitionToClose, ClosingTimeUuid);
        }

        private StatementExecutionResult ExecuteStatement(IStatement statement)
        {
            var statementExecutionResult = Session.Execute(statement).GetRows().Single();
            var isUpdateApplied = statementExecutionResult.GetValue<bool>("[applied]");
            if (isUpdateApplied) return new StatementExecutionResult {State = ExecutionState.Success};
            var partitionMaxTimeUuid = statementExecutionResult.GetValue<TimeUuid>("max_id_in_partition");
            return new StatementExecutionResult
            {
                State = partitionMaxTimeUuid == ClosingTimeUuid ? ExecutionState.PartitionClosed : ExecutionState.OutdatedId,
                PartitionMaxGuid = partitionMaxTimeUuid.ToTimeGuid()
            };
        }

        public override Event[] ReadRange(TimeGuid startExclusive, TimeGuid endInclusive, int count = 1000)
        {
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < OperationalTimeoutMilliseconds)
            {
                try
                {
                    return DoReadRange(startExclusive, endInclusive, count);
                }
                catch (Exception exception)
                {
                    Log.For(this).Error(exception, "Driver exception occured during read");
                    if (exception.IsCritical()) throw;
                }
            }

            throw new OperationTimeoutException(OperationalTimeoutMilliseconds);
        }

        private Event[] DoReadRange(TimeGuid startExclusive, TimeGuid endInclusive, int count)
        {
            if (startExclusive == null) startExclusive = startOfTimesHelper.StartOfTimes;

            return endInclusive == null 
                ? ReadFromStartToInfinity(startExclusive, count) 
                : ReadFromStartToEnd(startExclusive, endInclusive, count);
        }

        private Event[] ReadFromStartToEnd(TimeGuid startExclusive, TimeGuid endInclusive, int count)
        {
            var start = startExclusive.ToTimeUuid();
            var end = endInclusive.ToTimeUuid();

            var partitions = Partitioner
                .SplitIntoPartitions(startExclusive.GetTimestamp(), endInclusive.GetTimestamp())
                .Select(p => p.Ticks)
                .ToArray();

            return EventsTable
                .SetConsistencyLevel(ConsistencyLevel.Serial)
                .Where(x => partitions.Contains(x.PartitionId) && x.TimeUuid.CompareTo(start) > 0 && x.TimeUuid.CompareTo(end) <= 0)
                .Execute()
                .SelectMany(x => x.Select(e => new Event(x.TimeGuid, new EventProto(e.UserId, e.Payload))))
                .Take(count)
                .ToArray();
        }

        private Event[] ReadFromStartToInfinity(TimeGuid startExclusive, int count)
        {
            var extractedEvents = new List<Event>();
            var startingPartition = Partitioner.CreatePartitionId(startExclusive.GetTimestamp());

            var fistRead = MakeFirstRead(startExclusive, count, extractedEvents);

            if (!ShouldStopReading(fistRead))
                ContinueReading(Partitioner.Increment(startingPartition), count, extractedEvents);

            return extractedEvents.ToArray();
        }

        private IEnumerable<EventsCollection> MakeFirstRead(TimeGuid startExclusive, int count, List<Event> extractedEvents)
        {
            var startingPartition = Partitioner.CreatePartitionId(startExclusive.GetTimestamp());
            var startUuid = startExclusive.ToTimeUuid();

            var conditionalRead = ConditionalReadFromPartition(startUuid, startingPartition);
            extractedEvents.AddRange(ExtractEvents(conditionalRead, count - extractedEvents.Count));

            if (conditionalRead.Length != 0)
                return conditionalRead;

            var wholePartitionRead = UnconditionalReadFromPartition(startingPartition);
            extractedEvents.AddRange(ExtractEvents(wholePartitionRead, startExclusive, count - extractedEvents.Count));

            return wholePartitionRead;
        }

        private void ContinueReading(long partitionId, int count, List<Event> extractedEvents)
        {
            while (extractedEvents.Count < count)
            {
                var read = UnconditionalReadFromPartition(partitionId);
                extractedEvents.AddRange(ExtractEvents(read, count));

                if (ShouldStopReading(read)) return;

                partitionId = Partitioner.Increment(partitionId);
            }
        }

        private IEnumerable<Event> ExtractEvents(IEnumerable<EventsCollection> collection, TimeGuid startExclusive, int count)
        {
            return collection
                .Where(x => x.TimeUuid != default(TimeUuid) && x.TimeGuid > startExclusive)
                .SelectMany(x => x.Select(e => new Event(x.TimeGuid, new EventProto(e.UserId, e.Payload))))
                .Take(count);
        }

        private IEnumerable<Event> ExtractEvents(IEnumerable<EventsCollection> collection, int count)
        {
            return collection
                .Where(x => x.TimeUuid != default(TimeUuid))
                .SelectMany(x => x.Select(e => new Event(x.TimeGuid, new EventProto(e.UserId, e.Payload))))
                .Take(count);
        }

        private bool ShouldStopReading(IEnumerable<EventsCollection> read)
        {
            return read.All(x => x.MaxIdInPartition != ClosingTimeUuid);
        }

        /// <returns>If partition is empty, this method will return empty array.</returns>
        private EventsCollection[] ConditionalReadFromPartition(TimeUuid startExclusive, long partitionId)
        {
            return EventsTable
                .SetConsistencyLevel(ConsistencyLevel.Serial)
                .Where(x => x.PartitionId == partitionId && x.TimeUuid.CompareTo(startExclusive) > 0)
                .Execute()
                .ToArray();
        }

        /// <returns>
        /// If partition is empty, this method will return array with exactly one element 
        /// with data from static columns. Thx, Cassandra driver :/
        /// </returns>
        private EventsCollection[] UnconditionalReadFromPartition(long partitionId)
        {
            return EventsTable
                .SetConsistencyLevel(ConsistencyLevel.Serial)
                .Where(x => x.PartitionId == partitionId)
                .Execute()
                .ToArray();
        }

        private bool IsPartitionClosed(long partitionId)
        {
            return EventsTable
                .SetConsistencyLevel(ConsistencyLevel.Serial)
                .Where(x => x.PartitionId == partitionId)
                .Execute()
                .Any(x => x.MaxIdInPartition == ClosingTimeUuid);
        }

        private TimeGuid ReadLastWrittenTimeGuid()
        {
            var startGuid = lastWrittenTimeGuid ?? startOfTimesHelper.StartOfTimes;
            var partition = Partitioner.CreatePartitionId(startGuid.GetTimestamp());

            while (true)
            {
                var partitionId = partition;

                var read = EventsTable
                    .SetConsistencyLevel(ConsistencyLevel.Serial)
                    .Where(x => x.PartitionId == partitionId)
                    .Take(1)
                    .Execute()
                    .FirstOrDefault();

                if (read == null && !IsPartitionClosed(partitionId))
                    return TimeGuid.MinForTimestamp(new Timestamp(partitionId));

                if (read != null && read.MaxIdInPartition != ClosingTimeUuid)
                    return read.MaxIdInPartition.ToTimeGuid();

                partition = Partitioner.Increment(partition);
            }
        }
    }
}