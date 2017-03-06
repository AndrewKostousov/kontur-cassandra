using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Apache.Cassandra;
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
        protected readonly Table<Event> eventTable;
        private readonly Table<EventsCollection> bulkTable;
        protected readonly uint OperationsTimeoutMilliseconds;

        public SimpleTimeSeries(Table<Event> eventTable, Table<EventsCollection> bulkTable,  uint operationsTimeoutMilliseconds = 10000)
        {
            this.eventTable = eventTable;
            this.bulkTable = bulkTable;
            OperationsTimeoutMilliseconds = operationsTimeoutMilliseconds;
        }

        public virtual List<Timestamp> Write(params EventProto[] events)
        {
            var eventIds = events.Select(x => TimeGuid.NowGuid()).ToArray();
            var lastEventId = eventIds.Last();
            var timestamp = lastEventId.GetTimestamp();

            var eventsCollection = new EventsCollection
            {
                LastEventId = lastEventId.ToTimeUuid(),
                PartitionId = timestamp.Floor(Event.PartitionDutation).Ticks,
                EventIds = eventIds.Select(x => x.ToTimeUuid()).ToArray(),
                Payloads = events.Select(x => x.Payload).ToArray(),
                UserIds = events.Select(x => x.UserId).ToArray()
            };

            bulkTable.Insert(eventsCollection).Execute();

            return eventIds.Select(x => x.GetTimestamp()).ToList();
        }

        public virtual Timestamp Write(EventProto ev)
        {
            var eventToWrite = new Event(TimeGuid.NowGuid(), ev);
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < OperationsTimeoutMilliseconds)
            {
                try
                {
                    eventTable.Insert(eventToWrite).Execute();
                    return eventToWrite.Timestamp;
                }
                catch (DriverException ex)
                {
                    Logger.Log(ex);
                    if (!ShouldRetryAfter(ex)) throw;
                }
            }

            throw new OperationTimeoutException(OperationsTimeoutMilliseconds, eventToWrite);
        }

        protected static bool ShouldRetryAfter(DriverException ex)
        {
            return ex is QueryValidationException || ex is RequestInvalidException || ex is InvalidTypeException;
        }

        public void WriteWithoutSync(Event ev)
        {
            bulkTable.Insert(new EventsCollection
            {
                EventIds = new[] {ev.Id},
                LastEventId = ev.Id,
                PartitionId = ev.PartitionId,
                Payloads = new[] {ev.Payload},
                UserIds = new[] {ev.UserId},
            }).Execute();
        }

        public List<Event> ReadRange(Timestamp startExclusive, Timestamp endInclusive, int count = 1000)
        {
            var startGuid = startExclusive?.MaxTimeGuid();
            var endGuid = endInclusive?.MaxTimeGuid();

            return ReadRange(startGuid, endGuid, count);
        }

        public List<Event> ReadRange(TimeGuid startExclusive, TimeGuid endInclusive, int count = 1000)
        {
            var start = startExclusive?.ToTimeUuid();
            var end = endInclusive?.ToTimeUuid();

            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < OperationsTimeoutMilliseconds)
            {
                //try
                //{
                    if (!start.HasValue && !end.HasValue)
                        return ExtractEvents(bulkTable.Execute()).Take(count).ToList();

                    if (!start.HasValue)
                        return ExtractEvents(bulkTable.AllowFiltering().Where(ev => ev.LastEventId.CompareTo(end.Value) <= 0).Execute()).Take(count).ToList();

                    if (!end.HasValue)
                        return ExtractEventsAndFilter(bulkTable.AllowFiltering().Where(ev => ev.LastEventId.CompareTo(start.Value) >= 0).Execute(), startExclusive, count);

                    var slices = TimeSlicer
                        .Slice(startExclusive.GetTimestamp(), endInclusive.GetTimestamp(), Event.PartitionDutation)
                        .Select(s => s.Ticks)
                        .ToArray();

                    return ExtractEventsAndFilter(bulkTable
                        .Where(e => slices.Contains(e.PartitionId) && e.LastEventId.CompareTo(start.Value) >= 0 && e.LastEventId.CompareTo(end.Value) <= 0)
                        .Execute(), startExclusive, count);
                //}
                //catch (DriverException ex)
                //{
                //    Logger.Log(ex);
                //    if (!ShouldRetryAfter(ex)) throw;
                //}
            }

            throw new OperationTimeoutException(OperationsTimeoutMilliseconds);
        }

        private List<Event> ExtractEventsAndFilter(IEnumerable<EventsCollection> eventsCollection, TimeGuid startExclusive, int count)
        {
            return ExtractEvents(eventsCollection)
                .Where(x => x.Id.ToTimeGuid() > startExclusive)
                .Take(count)
                .ToList();
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