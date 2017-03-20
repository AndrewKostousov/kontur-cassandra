using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class SimpleTimeSeries : ITimeSeries
    {
        public TimeLinePartitioner Partitioner { get; }

        protected readonly Table<EventsCollection> eventsTable;
        protected readonly uint OperationalTimeoutMilliseconds;

        public SimpleTimeSeries(Table<EventsCollection> eventsTable, TimeLinePartitioner partitioner, uint operationalTimeoutMilliseconds = 10000)
        {
            this.eventsTable = eventsTable;
            OperationalTimeoutMilliseconds = operationalTimeoutMilliseconds;
            Partitioner = partitioner;
        }

        public virtual Timestamp[] Write(params EventProto[] events)
        {
            var eventsToWrite = PackIntoCollection(events, TimeGuid.NowGuid);

            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < OperationalTimeoutMilliseconds)
            {
                try
                {
                    eventsTable.Insert(eventsToWrite).Execute();
                    return eventsToWrite.Select(x => x.Timestamp).ToArray();
                }
                catch (DriverException ex)
                {
                    Logger.Log(ex);
                    if (IsCriticalError(ex)) throw;
                }
            }

            throw new OperationTimeoutException(OperationalTimeoutMilliseconds);
        }

        protected EventsCollection PackIntoCollection(EventProto[] eventProtos, Func<TimeGuid> createGuid)
        {
            var collection = new EventsCollection
            {
                Payloads = eventProtos.Select(x => x.Payload).ToArray(),
                UserIds = eventProtos.Select(x => x.UserId).ToArray()
            };

            return UpdateEventCollectionIds(collection, createGuid);
        }

        protected EventsCollection UpdateEventCollectionIds(EventsCollection oldCollection, Func<TimeGuid> createGuid)
        {
            var eventIds = oldCollection.UserIds.Select(_ => createGuid()).ToArray();
            var lastEventId = eventIds.Last();

            oldCollection.EventIds = eventIds.Select(x => x.ToTimeUuid()).ToArray();
            oldCollection.LastEventId = lastEventId.ToTimeUuid();
            oldCollection.PartitionId = Partitioner.CreatePartitionId(lastEventId.GetTimestamp());

            return oldCollection;
        }

        protected static bool IsCriticalError(DriverException ex)
        {
            return ex is QueryValidationException || ex is RequestInvalidException || ex is InvalidTypeException;
        }

        public void WriteWithoutSync(Event ev)
        {
            eventsTable.Insert(new EventsCollection
            {
                EventIds = new[] {ev.Id},
                LastEventId = ev.Id,
                PartitionId = Partitioner.CreatePartitionId(ev.TimeGuid.GetTimestamp()),
                Payloads = new[] {ev.Proto.Payload},
                UserIds = new[] {ev.Proto.UserId},
            }).Execute();
        }

        public Event[] ReadRange(Timestamp startExclusive, Timestamp endInclusive, int count = 1000)
        {
            var startGuid = startExclusive?.MaxTimeGuid();
            var endGuid = endInclusive?.MaxTimeGuid();

            return ReadRange(startGuid, endGuid, count);
        }

        public Event[] ReadRange(TimeGuid startExclusive, TimeGuid endInclusive, int count = 1000)
        {
            var start = startExclusive?.ToTimeUuid();
            var end = endInclusive?.ToTimeUuid();

            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < OperationalTimeoutMilliseconds)
            {
                try
                {
                    if (!start.HasValue && !end.HasValue)
                        return ExtractEvents(eventsTable.Execute()).Take(count).ToArray();

                    if (!start.HasValue)
                        return ExtractEvents(eventsTable.AllowFiltering().Where(ev => ev.LastEventId.CompareTo(end.Value) <= 0).Execute()).Take(count).ToArray();

                    if (!end.HasValue)
                        return ExtractEventsAndFilter(eventsTable.AllowFiltering().Where(ev => ev.LastEventId.CompareTo(start.Value) >= 0).Execute(), startExclusive, count);

                    var slices = Partitioner
                        .SplitIntoPartitions(startExclusive.GetTimestamp(), endInclusive.GetTimestamp())
                        .Select(s => s.Ticks)
                        .ToArray();

                    return ExtractEventsAndFilter(eventsTable
                        .Where(e => slices.Contains(e.PartitionId) && e.LastEventId.CompareTo(start.Value) >= 0 && e.LastEventId.CompareTo(end.Value) <= 0)
                        .Execute(), startExclusive, count);
                }
                catch (DriverException ex)
                {
                    Logger.Log(ex);
                    if (IsCriticalError(ex)) throw;
                }
            }

            throw new OperationTimeoutException(OperationalTimeoutMilliseconds);
        }

        private Event[] ExtractEventsAndFilter(IEnumerable<EventsCollection> eventsCollection, TimeGuid startExclusive, int count)
        {
            return ExtractEvents(eventsCollection)
                .Where(x => x.Id.ToTimeGuid() > startExclusive)
                .Take(count)
                .ToArray();
        }

        private IEnumerable<Event> ExtractEvents(IEnumerable<EventsCollection> eventsCollections)
        {
            return eventsCollections
                .Where(e => e.LastEventId.ToGuid() != Guid.Empty)
                .OrderBy(x => x.LastEventId)
                .SelectMany(x => x);
        }
    }
}