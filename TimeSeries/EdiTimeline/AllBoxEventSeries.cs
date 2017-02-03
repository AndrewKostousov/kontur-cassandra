using System;
using System.Collections.Generic;
using System.Linq;
using Commons;
using Commons.Logging;
using GroBuf;
using JetBrains.Annotations;
using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace EdiTimeline
{
    public class AllBoxEventSeries : IAllBoxEventSeries
    {
        public AllBoxEventSeries(IAllBoxEventSeriesSettings settings, ISerializer serializer, IAllBoxEventSeriesTicksHolder allBoxEventSeriesTicksHolder, ICassandraCluster cassandraCluster)
        {
            this.settings = settings;
            this.serializer = serializer;
            this.allBoxEventSeriesTicksHolder = allBoxEventSeriesTicksHolder;
            eventsConnection = cassandraCluster.RetrieveColumnFamilyConnection(BoxEventSeriesCassandraSchemaConfigurator.BoxEventSeriesKeyspace, BoxEventSeriesCassandraSchemaConfigurator.AllBoxEventSeriesEventsColumnFamily);
        }

        public TimeSpan PartitionDuration => settings.PartitionDuration;

        public void WriteEventsInAnyOrder([NotNull] List<AllBoxEventSeriesWriterQueueItem> queueItems)
        {
            try
            {
                var nowTicks = Timestamp.Now.Ticks;
                allBoxEventSeriesTicksHolder.SetEventSeriesExclusiveStartTicks(nowTicks - 1);
                var lastGoodEventTicks = allBoxEventSeriesTicksHolder.GetLastGoodEventTicks() ?? Timestamp.MinValue.Ticks;
                var firstEventTicks = Math.Max(nowTicks, lastGoodEventTicks + 1);
                var entries = queueItems.Select((x, idx) =>
                {
                    // todo (avk, 08.01.2017): возможно, тут стоит тоже не по тику, а по микросекунде добавлять
                    var eventTimestamp = new Timestamp(firstEventTicks + idx);
                    return new
                        {
                            QueueItem = x,
                            EventTicks = eventTimestamp.Ticks,
                            PartitionKey = AllBoxEventSeriesCassandraHelpers.FormatPartitionKey(eventTimestamp, settings.PartitionDuration),
                            ColumnName = AllBoxEventSeriesCassandraHelpers.FormatColumnName(eventTimestamp, x.ProtoBoxEvent.EventId),
                        };
                }).ToArray();
                eventsConnection.BatchInsert(entries.GroupBy(x => x.PartitionKey)
                                                    .Select(g => new KeyValuePair<string, IEnumerable<Column>>(g.Key, g.Select(x => new Column
                                                        {
                                                            Name = x.ColumnName,
                                                            Value = serializer.Serialize(x.QueueItem.GetAllBoxEventSeriesColumnValue(eventIsCommitted: false)),
                                                            Timestamp = x.EventTicks,
                                                            TTL = (int)settings.NotCommittedEventsTtl.TotalSeconds,
                                                        }))));
                lastGoodEventTicks = allBoxEventSeriesTicksHolder.GetLastGoodEventTicks() ?? Timestamp.MinValue.Ticks;
                var badEntries = entries.Where(x => x.EventTicks <= lastGoodEventTicks).ToArray();
                var goodEntries = entries.Where(x => x.EventTicks > lastGoodEventTicks).ToArray();
                if (badEntries.Any())
                {
                    var badEventColumnNamesByPartition = badEntries.GroupBy(x => x.PartitionKey)
                                                                   .Select(g => new KeyValuePair<string, IEnumerable<string>>(g.Key, g.Select(x => x.ColumnName)));
                    eventsConnection.BatchDelete(badEventColumnNamesByPartition, badEntries.Last().EventTicks + 1);
                }
                if (goodEntries.Any())
                {
                    allBoxEventSeriesTicksHolder.SetLastGoodEventTicks(goodEntries.Last().EventTicks);
                    eventsConnection.BatchInsert(goodEntries.GroupBy(x => x.PartitionKey)
                                                            .Select(g => new KeyValuePair<string, IEnumerable<Column>>(g.Key, g.Select(x => new Column
                                                                {
                                                                    Name = x.ColumnName,
                                                                    Value = serializer.Serialize(x.QueueItem.GetAllBoxEventSeriesColumnValue(eventIsCommitted: true)),
                                                                    Timestamp = x.EventTicks + 1,
                                                                    TTL = null,
                                                                }))));
                }
                foreach (var entry in entries)
                    entry.QueueItem.EventTimestamp.SetResult(entry.EventTicks > lastGoodEventTicks ? new Timestamp(entry.EventTicks) : null);
            }
            catch (Exception e)
            {
                Log.For(this).Error(e, "Failed to process AllBoxEventSeries writer queue items: {queueItemsCount}", queueItems.Count);
                foreach (var item in queueItems)
                    item.EventTimestamp.SetResult(null);
            }
        }

        public void WriteEventsWithNoSynchronization([NotNull] params BoxEvent[] boxEvents)
        {
            if (!boxEvents.Any())
                return;
            allBoxEventSeriesTicksHolder.SetEventSeriesExclusiveStartTicks(boxEvents.Select(x => x.EventTimestamp).Min().Ticks - 1);
            eventsConnection.BatchInsert(boxEvents.GroupBy(x => AllBoxEventSeriesCassandraHelpers.FormatPartitionKey(x.EventTimestamp, settings.PartitionDuration))
                                                  .Select(g => new KeyValuePair<string, IEnumerable<Column>>(g.Key, g.Select(x => new Column
                                                      {
                                                          Name = AllBoxEventSeriesCassandraHelpers.FormatColumnName(x.EventTimestamp, x.EventId),
                                                          Value = serializer.Serialize(new AllBoxEventSeriesColumnValue(x.Payload, eventIsCommitted: true)),
                                                          Timestamp = x.EventTimestamp.Ticks,
                                                          TTL = null,
                                                      }))));
        }

        [CanBeNull]
        public AllBoxEventSeriesRange TryCreateRange([CanBeNull] AllBoxEventSeriesPointer exclusiveStartEventPointer, [CanBeNull] Timestamp inclusiveEndTimestamp)
        {
            Guid exclusiveStartEventId;
            Timestamp exclusiveStartTimestamp;
            if (exclusiveStartEventPointer != null)
            {
                exclusiveStartEventId = exclusiveStartEventPointer.EventId;
                exclusiveStartTimestamp = exclusiveStartEventPointer.EventTimestamp;
            }
            else
            {
                var eventSeriesExclusiveStartTicks = allBoxEventSeriesTicksHolder.GetEventSeriesExclusiveStartTicks();
                if (!eventSeriesExclusiveStartTicks.HasValue)
                    return null;
                exclusiveStartEventId = GuidHelpers.MaxGuid;
                exclusiveStartTimestamp = new Timestamp(eventSeriesExclusiveStartTicks.Value);
            }
            if (inclusiveEndTimestamp == null)
            {
                var lastGoodEventTicks = allBoxEventSeriesTicksHolder.GetLastGoodEventTicks();
                if (!lastGoodEventTicks.HasValue)
                    return null;
                inclusiveEndTimestamp = new Timestamp(lastGoodEventTicks.Value);
            }
            if (exclusiveStartTimestamp > inclusiveEndTimestamp)
                return null;
            return new AllBoxEventSeriesRange(exclusiveStartTimestamp, exclusiveStartEventId, inclusiveEndTimestamp, settings.PartitionDuration);
        }

        [CanBeNull]
        public AllBoxEventSeriesRange TryCreateRange([CanBeNull] Timestamp exclusiveStartTimestamp, [CanBeNull] Timestamp inclusiveEndTimestamp)
        {
            var eventSeriesExclusiveStartTicks = allBoxEventSeriesTicksHolder.GetEventSeriesExclusiveStartTicks();
            if (!eventSeriesExclusiveStartTicks.HasValue)
                return null;
            var eventSeriesExclusiveStartTimestamp = new Timestamp(eventSeriesExclusiveStartTicks.Value);
            if (exclusiveStartTimestamp == null || eventSeriesExclusiveStartTimestamp > exclusiveStartTimestamp)
                exclusiveStartTimestamp = eventSeriesExclusiveStartTimestamp;
            if (inclusiveEndTimestamp == null)
            {
                var lastGoodEventTicks = allBoxEventSeriesTicksHolder.GetLastGoodEventTicks();
                if (!lastGoodEventTicks.HasValue)
                    return null;
                inclusiveEndTimestamp = new Timestamp(lastGoodEventTicks.Value);
            }
            if (exclusiveStartTimestamp > inclusiveEndTimestamp)
                return null;
            return new AllBoxEventSeriesRange(exclusiveStartTimestamp, GuidHelpers.MaxGuid, inclusiveEndTimestamp, settings.PartitionDuration);
        }

        [NotNull]
        public List<TResultBoxEvent> ReadEvents<TResultBoxEvent>([CanBeNull] AllBoxEventSeriesRange range, int take, [NotNull] Func<BoxEvent[], TResultBoxEvent[]> convertAndFilter)
        {
            if (range == null || take <= 0)
                return new List<TResultBoxEvent>();
            if (take == int.MaxValue)
                take--;
            var eventsToFetch = take;
            var events = new List<TResultBoxEvent>();
            do
            {
                var columnsToFetch = Math.Max(eventsToFetch, settings.MinBatchSizeForRead) + 1;
                var columnsIncludingStartColumn = eventsConnection.GetColumns(range.StartPartitionKey, range.ExclusiveStartColumnName, range.InclusiveEndColumnName, columnsToFetch, reversed: false);
                var columns = columnsIncludingStartColumn.SkipWhile(x => x.Name == range.ExclusiveStartColumnName).ToArray();
                var boxEvents = columns.Select(x => new {EventSeriesPointer = AllBoxEventSeriesCassandraHelpers.ParseColumnName(x.Name), ColumnValue = serializer.Deserialize<AllBoxEventSeriesColumnValue>(x.Value)})
                                       .TakeWhile(x => x.ColumnValue.EventIsCommitted)
                                       .Select(x => new BoxEvent(x.EventSeriesPointer.EventId, x.EventSeriesPointer.EventTimestamp, x.ColumnValue.Payload))
                                       .ToArray();
                events.AddRange(convertAndFilter(boxEvents).Take(eventsToFetch));
                var notCommittedEventIsReached = boxEvents.Length < columns.Length;
                var currentPartitionIsExhausted = columnsIncludingStartColumn.Length < columnsToFetch;
                range = range.MoveNext(notCommittedEventIsReached, currentPartitionIsExhausted, columns.LastOrDefault()?.Name, settings.PartitionDuration);
                eventsToFetch = take - events.Count;
            } while (range != null && eventsToFetch > 0);
            return events;
        }

        private readonly IAllBoxEventSeriesSettings settings;
        private readonly ISerializer serializer;
        private readonly IAllBoxEventSeriesTicksHolder allBoxEventSeriesTicksHolder;
        private readonly IColumnFamilyConnection eventsConnection;
    }
}