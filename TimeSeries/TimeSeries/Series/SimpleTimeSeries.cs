using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.Logging;
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
            var eventsToWrite = PackIntoCollection(events);

            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < OperationalTimeoutMilliseconds)
            {
                try
                {
                    eventsTable.Insert(eventsToWrite).Execute();
                    return eventsToWrite.Select(_ => eventsToWrite.TimeGuid.GetTimestamp()).ToArray();
                }
                catch (DriverException ex)
                {
                    Log.For(this).Error(ex, "Cassandra driver exception occured during write.");
                    if (ex.IsCritical()) throw;
                }
            }

            throw new OperationTimeoutException(OperationalTimeoutMilliseconds);
        }

        private EventsCollection PackIntoCollection(IEnumerable<EventProto> eventProtos)
        {
            var id = TimeGuid.NowGuid();
            var partitionId = Partitioner.CreatePartitionId(id.GetTimestamp());

            return new EventsCollection(id, partitionId, eventProtos);
        }

        public void WriteWithoutSync(Event ev)
        {
            eventsTable.Insert(new EventsCollection(ev.Id, Partitioner.CreatePartitionId(ev.TimeGuid.GetTimestamp()), ev.Proto)).Execute();
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
                        return ExtractEvents(eventsTable.AllowFiltering().Where(ev => ev.TimeUuid.CompareTo(end.Value) <= 0).Execute()).Take(count).ToArray();

                    if (!end.HasValue)
                        return ExtractEventsAndFilter(eventsTable.AllowFiltering().Where(ev => ev.TimeUuid.CompareTo(start.Value) >= 0).Execute(), startExclusive, count);

                    var slices = Partitioner
                        .SplitIntoPartitions(startExclusive.GetTimestamp(), endInclusive.GetTimestamp())
                        .Select(s => s.Ticks)
                        .ToArray();

                    if (slices.Length == 0) return new Event[0];

                    return ExtractEventsAndFilter(eventsTable
                        .Where(e => slices.Contains(e.PartitionId) && e.TimeUuid.CompareTo(start.Value) >= 0 && e.TimeUuid.CompareTo(end.Value) <= 0)
                        .Execute(), startExclusive, count);
                }
                catch (DriverException ex)
                {
                    Log.For(this).Error(ex, "Cassandra driver exception occured during write.");
                    if (ex.IsCritical()) throw;
                }
            }

            throw new OperationTimeoutException(OperationalTimeoutMilliseconds);
        }

        private Event[] ExtractEventsAndFilter(IEnumerable<EventsCollection> eventsCollections, TimeGuid startExclusive, int count)
        {
            return ExtractEvents(eventsCollections)
                .Where(e => e.TimeGuid > startExclusive)
                .Take(count)
                .ToArray();
        }

        private IEnumerable<Event> ExtractEvents(IEnumerable<EventsCollection> eventsCollections)
        {
            return eventsCollections
                .Where(e => e.TimeUuid.ToGuid() != Guid.Empty)
                .OrderBy(x => x.TimeUuid)
                .SelectMany(x => x.Select(e => new Event(x.TimeGuid, e)));
        }
    }
}