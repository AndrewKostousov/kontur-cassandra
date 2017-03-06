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
        protected readonly uint OperationsTimeoutMilliseconds;

        public SimpleTimeSeries(Table<Event> eventTable, uint operationsTimeoutMilliseconds = 1000)
        {
            this.eventTable = eventTable;
            OperationsTimeoutMilliseconds = operationsTimeoutMilliseconds;
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
            eventTable.Insert(ev).Execute();
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
                try
                {
                    if (!start.HasValue && !end.HasValue)
                        return eventTable.Execute().OrderBy(ev => ev.Id).Take(count).ToList();

                    if (!start.HasValue)
                        return GetFromTableAndSort(count, ev => ev.Id.CompareTo(end.Value) <= 0);

                    if (!end.HasValue)
                        return GetFromTableAndSort(count, ev => ev.Id.CompareTo(start.Value) > 0);

                    var slices = TimeSlicer
                        .Slice(startExclusive.GetTimestamp(), endInclusive.GetTimestamp(), Event.PartitionDutation)
                        .Select(s => s.Ticks);

                    return eventTable
                        .Where(
                            e =>
                                slices.Contains(e.PartitionId) && e.Id.CompareTo(start.Value) > 0 &&
                                e.Id.CompareTo(end.Value) <= 0)
                        .Take(count)
                        .Execute()
                        .Where(e => e.Id.ToGuid() != Guid.Empty)
                        .ToList();
                }
                catch (DriverException ex)
                {
                    Logger.Log(ex);
                    if (!ShouldRetryAfter(ex)) throw;
                }
            }

            throw new OperationTimeoutException(OperationsTimeoutMilliseconds);
        }

        private List<Event> GetFromTableAndSort(int count, Expression<Func<Event, bool>> query)
        {
            return eventTable
                .AllowFiltering()
                .Where(query)
                .Execute()
                .Where(e => e.Id.ToGuid() != Guid.Empty)
                .OrderBy(x => x.Id)
                .Take(count)
                .ToList();
        }
    }
}