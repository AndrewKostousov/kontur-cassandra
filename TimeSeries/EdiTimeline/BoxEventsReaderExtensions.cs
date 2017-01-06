using System;
using System.Collections.Generic;
using System.Linq;
using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public static class BoxEventsReaderExtensions
    {
        [NotNull]
        public static IEnumerable<List<BoxEvent>> ReadEventsToEnd([NotNull] this IBoxEventsReader boxEventsReader, Guid? exclusiveStartEventId, int batchSize = defaultBatchSize)
        {
            var inclusiveEndTimestamp = Timestamp.Now;
            while (true)
            {
                bool exclusiveStartEventNotFound;
                var eventsRange = boxEventsReader.TryCreateEventSeriesRange(exclusiveStartEventId, inclusiveEndTimestamp, out exclusiveStartEventNotFound);
                if (exclusiveStartEventNotFound)
                    throw new InvalidProgramStateException(string.Format("Exclusive start event not found: {0}", exclusiveStartEventId));
                var batch = boxEventsReader.ReadEvents(eventsRange, batchSize, x => x);
                if (batch.Any())
                    yield return batch;
                if (batch.Count < batchSize)
                    break;
                exclusiveStartEventId = batch.Last().EventId;
            }
        }

        [NotNull]
        public static IEnumerable<List<BoxEvent>> ReadEventsToEnd([NotNull] this IBoxEventsReader boxEventsReader, [NotNull] Timestamp exclusiveStartTimestamp, int batchSize = defaultBatchSize)
        {
            var firstEventsRange = boxEventsReader.TryCreateEventSeriesRange(exclusiveStartTimestamp, Timestamp.Now);
            var firstBatch = boxEventsReader.ReadEvents(firstEventsRange, batchSize, x => x);
            if (firstBatch.Any())
                yield return firstBatch;
            if (firstBatch.Count < batchSize)
                yield break;
            var exclusiveStartEventId = firstBatch.Last().EventId;
            foreach (var batch in boxEventsReader.ReadEventsToEnd(exclusiveStartEventId, batchSize))
                yield return batch;
        }

        private const int defaultBatchSize = 5000;
    }
}