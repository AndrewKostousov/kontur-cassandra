using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Series;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.Logging;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class SimpleTimeSeries : BaseTimeSeries
    {
        public SimpleTimeSeries(SimpleTimeSeriesDatabaseController databaseController, TimeLinePartitioner partitioner, uint operationalTimeoutMilliseconds = 10000)
            : base(databaseController.EventsTable, partitioner, operationalTimeoutMilliseconds) { }

        public override void WriteWithoutSync(Event ev)
        {
            EventsTable.Insert(new EventsCollection(ev.Id, Partitioner.CreatePartitionId(ev.TimeGuid.GetTimestamp()), ev.Proto)).Execute();
        }

        public override Timestamp[] Write(params EventProto[] events)
        {
            var eventsToWrite = PackIntoCollection(events, NewTimeGuid());
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < OperationalTimeoutMilliseconds)
            {
                try
                {
                    EventsTable.Insert(eventsToWrite).Execute();
                    return eventsToWrite.Select(_ => eventsToWrite.TimeGuid.GetTimestamp()).ToArray();
                }
                catch (Exception ex)
                {
                    Log.For(this).Error(ex, "Cassandra driver exception occured during write.");
                    if (ex.IsCritical()) throw;
                }
            }

            throw new OperationTimeoutException(OperationalTimeoutMilliseconds);
        }

        public override Event[] ReadRange(TimeGuid startExclusive, TimeGuid endInclusive, int count = 1000)
        {
            var start = startExclusive?.ToTimeUuid();
            var end = endInclusive?.ToTimeUuid();

            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < OperationalTimeoutMilliseconds)
            {
                try
                {
                    if (!start.HasValue && !end.HasValue)
                        return ExtractEvents(EventsTable.Execute()).Take(count).ToArray();

                    if (!start.HasValue)
                        return ExtractEvents(EventsTable.AllowFiltering().Where(ev => ev.TimeUuid.CompareTo(end.Value) <= 0).Execute()).Take(count).ToArray();

                    if (!end.HasValue)
                        return ExtractEventsAndFilter(EventsTable.AllowFiltering().Where(ev => ev.TimeUuid.CompareTo(start.Value) >= 0).Execute(), startExclusive, count);

                    var slices = Partitioner
                        .SplitIntoPartitions(startExclusive.GetTimestamp(), endInclusive.GetTimestamp())
                        .Select(s => s.Ticks)
                        .ToArray();

                    if (slices.Length == 0) return new Event[0];

                    return ExtractEventsAndFilter(EventsTable
                        .Where(e => slices.Contains(e.PartitionId) && e.TimeUuid.CompareTo(start.Value) >= 0 && e.TimeUuid.CompareTo(end.Value) <= 0)
                        .Execute(), startExclusive, count);
                }
                catch (Exception ex)
                {
                    Log.For(this).Error(ex, "Cassandra driver exception occured during read.");
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