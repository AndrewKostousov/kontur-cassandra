using Cassandra;
using Cassandra.Data.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Commons;
using Commons.TimeBasedUuid;
using SKBKontur.Catalogue.CassandraStorageCore.CqlCore;

namespace CassandraTimeSeries
{
    public class TimeSeries : ITimeSeries
    {
        private readonly Table<Event> table;
        
        public TimeSeries(Table<Event> table)
        {
            this.table = table;
        }

        public void Write(Event ev)
        {
            table.Insert(ev).Execute();
        }

        public List<Event> ReadRange(Timestamp startInclusive, Timestamp endExclusive, int count = 1000)
        {
            var startGuid = startInclusive?.MinTimeGuid();

            var endGuid = startInclusive == endExclusive 
                ? endExclusive?.MaxTimeGuid()
                : endExclusive?.MinTimeGuid();
            
            return ReadRange(startGuid, endGuid, count);
        }

        public List<Event> ReadRange(TimeGuid startInclusive, TimeGuid endExclusive, int count = 1000)
        {
            var start = startInclusive ?? TimeGuid.MinValue;
            var end = endExclusive ?? TimeGuid.MaxValue;
            
            return TimeSlicer.Slice(start.GetTimestamp(), end.GetTimestamp(), Event.SliceDutation)
                .Select(sliceId => GetIdCondition(startInclusive?.ToTimeUuid(), endExclusive?.ToTimeUuid(), sliceId.Ticks))
                .SelectMany(condition => GetRangeFromTable(condition, count))
                .Take(count)
                .ToList();
        }

        private IEnumerable<Event> GetRangeFromTable(Expression<Func<Event, bool>> condition, int count)
        {
            return table
                .Where(condition)
                .Take(count)
                .Execute();
        }

        private Expression<Func<Event, bool>> GetIdCondition(TimeUuid? start, TimeUuid? end, long sliceId)
        {
            if (start == null && end == null)
                return e => e.SliceId == sliceId;
            if (end == null)
                return e => e.SliceId == sliceId && e.Id.CompareTo(start.Value) >= 0;
            if (start == null)
                return e => e.SliceId == sliceId && e.Id.CompareTo(end.Value) < 0;
            if (start == end)
                return e => e.SliceId == sliceId && e.Id.CompareTo(start.Value) == 0;

            return e => e.SliceId == sliceId && e.Id.CompareTo(start.Value) >= 0 && e.Id.CompareTo(end.Value) < 0;
        }
    }
}