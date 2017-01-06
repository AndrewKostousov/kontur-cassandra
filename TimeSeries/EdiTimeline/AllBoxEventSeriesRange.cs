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
                throw new InvalidProgramStateException(string.Format("ExclusiveStartColumnName.Timestamp ({0}) > inclusiveEndTimestamp ({1}); startPartitionKey: {2}", exclusiveStartColumnName, inclusiveEndTimestamp, startPartitionKey));
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
        public string StartPartitionKey { get; private set; }

        [CanBeNull]
        public string ExclusiveStartColumnName { get; private set; }

        [NotNull]
        public string InclusiveEndColumnName
        {
            get { return AllBoxEventSeriesCassandraHelpers.FormatColumnName(inclusiveEndTimestamp.Ticks, GuidHelpers.MaxGuid); }
        }

        public override string ToString()
        {
            return string.Format("StartPartitionKey: {0}, ExclusiveStartColumnName: {1}, InclusiveEndEventTimestamp: {2}", StartPartitionKey, ExclusiveStartColumnName, inclusiveEndTimestamp);
        }

        private readonly Timestamp inclusiveEndTimestamp;
    }
}