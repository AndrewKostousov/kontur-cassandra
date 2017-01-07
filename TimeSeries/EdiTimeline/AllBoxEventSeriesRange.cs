using System;
using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class AllBoxEventSeriesRange
    {
        public AllBoxEventSeriesRange([NotNull] Timestamp exclusiveStartTimestamp, Guid exclusiveStartEventId, [NotNull] Timestamp inclusiveEndTimestamp, TimeSpan partitionDuration)
            : this(AllBoxEventSeriesCassandraHelpers.FormatPartitionKey(exclusiveStartTimestamp.Ticks, partitionDuration), AllBoxEventSeriesCassandraHelpers.FormatColumnName(exclusiveStartTimestamp.Ticks, exclusiveStartEventId), inclusiveEndTimestamp)
        {
        }

        private AllBoxEventSeriesRange([NotNull] string startPartitionKey, [CanBeNull] string exclusiveStartColumnName, [NotNull] Timestamp inclusiveEndTimestamp)
        {
            if (string.IsNullOrEmpty(startPartitionKey))
                throw new InvalidProgramStateException("startPartitionKey is empty");
            if (exclusiveStartColumnName != null && AllBoxEventSeriesCassandraHelpers.ParseColumnName(exclusiveStartColumnName).EventTimestamp > inclusiveEndTimestamp)
                throw new InvalidProgramStateException($"ExclusiveStartColumnName.Timestamp ({exclusiveStartColumnName}) > inclusiveEndTimestamp ({inclusiveEndTimestamp}); startPartitionKey: {startPartitionKey}");
            StartPartitionKey = startPartitionKey;
            ExclusiveStartColumnName = exclusiveStartColumnName;
            this.inclusiveEndTimestamp = inclusiveEndTimestamp;
        }

        [CanBeNull]
        public AllBoxEventSeriesRange MoveNext(bool notCommittedEventIsReached, bool currentPartitionIsExhausted, [CanBeNull] string lastFetchedColumnName, TimeSpan partitionDuration)
        {
            if (notCommittedEventIsReached)
                return null;
            if (lastFetchedColumnName != null && AllBoxEventSeriesCassandraHelpers.ParseColumnName(lastFetchedColumnName).EventTimestamp > inclusiveEndTimestamp)
                return null;
            if (currentPartitionIsExhausted)
            {
                var nextPartitionKey = AllBoxEventSeriesCassandraHelpers.NextPartitionKey(StartPartitionKey, partitionDuration);
                if (AllBoxEventSeriesCassandraHelpers.ParsePartitionKey(nextPartitionKey) > inclusiveEndTimestamp)
                    return null;
                return new AllBoxEventSeriesRange(nextPartitionKey, null, inclusiveEndTimestamp);
            }
            return new AllBoxEventSeriesRange(StartPartitionKey, lastFetchedColumnName, inclusiveEndTimestamp);
        }

        [NotNull]
        public string StartPartitionKey { get; }

        [CanBeNull]
        public string ExclusiveStartColumnName { get; }

        [NotNull]
        public string InclusiveEndColumnName => AllBoxEventSeriesCassandraHelpers.FormatColumnName(inclusiveEndTimestamp.Ticks, GuidHelpers.MaxGuid);

        public override string ToString()
        {
            return $"StartPartitionKey: {StartPartitionKey}, ExclusiveStartColumnName: {ExclusiveStartColumnName}, InclusiveEndEventTimestamp: {inclusiveEndTimestamp}";
        }

        private readonly Timestamp inclusiveEndTimestamp;
    }
}