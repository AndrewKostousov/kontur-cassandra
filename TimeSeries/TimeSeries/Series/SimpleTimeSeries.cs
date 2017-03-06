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
        protected readonly Table<EventsCollection> eventsTable;
        protected readonly uint OperationsTimeoutMilliseconds;

        public SimpleTimeSeries(Table<EventsCollection> eventsTable,  uint operationsTimeoutMilliseconds = 10000)
        {
            this.eventsTable = eventsTable;
            OperationsTimeoutMilliseconds = operationsTimeoutMilliseconds;
        }

        public virtual Timestamp[] Write(params EventProto[] events)
        {
            var eventsToWrite = PackIntoCollection(events, TimeGuid.NowGuid);

            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < OperationsTimeoutMilliseconds)
            {
                try
                {
                    eventsTable.Insert(eventsToWrite).Execute();
                    return eventsToWrite.Select(x => x.Timestamp).ToArray();
                }
                catch (DriverException ex)
                {
                    Logger.Log(ex);
                    if (!ShouldRetryAfter(ex)) throw;
                }
            }

            throw new OperationTimeoutException(OperationsTimeoutMilliseconds);
        }

        protected static EventsCollection PackIntoCollection(EventProto[] eventProtos, Func<TimeGuid> createGuid)
        {
            var eventIds = eventProtos.Select(x => createGuid()).ToArray();
            var lastEventId = eventIds.Last();
            var timestamp = lastEventId.GetTimestamp();

            return new EventsCollection
            {
                LastEventId = lastEventId.ToTimeUuid(),
                PartitionId = timestamp.Floor(Event.PartitionDutation).Ticks,
                EventIds = eventIds.Select(x => x.ToTimeUuid()).ToArray(),
                Payloads = eventProtos.Select(x => x.Payload).ToArray(),
                UserIds = eventProtos.Select(x => x.UserId).ToArray()
            };
        }

        protected static bool ShouldRetryAfter(DriverException ex)
        {
            return !(ex is QueryValidationException || ex is RequestInvalidException || ex is InvalidTypeException);
        }

        public void WriteWithoutSync(Event ev)
        {
            eventsTable.Insert(new EventsCollection
            {
                EventIds = new[] {ev.Id},
                LastEventId = ev.Id,
                PartitionId = ev.PartitionId,
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

            while (sw.ElapsedMilliseconds < OperationsTimeoutMilliseconds)
            {
                try
                {
                    if (!start.HasValue && !end.HasValue)
                        return ExtractEvents(eventsTable.Execute()).Take(count).ToArray();

                    if (!start.HasValue)
                        return ExtractEvents(eventsTable.AllowFiltering().Where(ev => ev.LastEventId.CompareTo(end.Value) <= 0).Execute()).Take(count).ToArray();

                    if (!end.HasValue)
                        return ExtractEventsAndFilter(eventsTable.AllowFiltering().Where(ev => ev.LastEventId.CompareTo(start.Value) >= 0).Execute(), startExclusive, count);

                    var slices = TimeSlicer
                        .Slice(startExclusive.GetTimestamp(), endInclusive.GetTimestamp(), Event.PartitionDutation)
                        .Select(s => s.Ticks)
                        .ToArray();

                    return ExtractEventsAndFilter(eventsTable
                        .Where(e => slices.Contains(e.PartitionId) && e.LastEventId.CompareTo(start.Value) >= 0 && e.LastEventId.CompareTo(end.Value) <= 0)
                        .Execute(), startExclusive, count);
                }
                catch (DriverException ex)
                {
                    Logger.Log(ex);
                    if (!ShouldRetryAfter(ex)) throw;
                }
            }

            throw new OperationTimeoutException(OperationsTimeoutMilliseconds);
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