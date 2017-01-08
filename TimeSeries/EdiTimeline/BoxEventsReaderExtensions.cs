using System.Collections.Generic;
using System.Linq;
using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public static class BoxEventsReaderExtensions
    {
        [NotNull]
        public static IEnumerable<List<BoxEvent>> ReadEventsToEnd([NotNull] this IBoxEventsReader boxEventsReader, [CanBeNull] AllBoxEventSeriesPointer exclusiveStartEventPointer, int batchSize = defaultBatchSize)
        {
            var inclusiveEndTimestamp = Timestamp.Now;
            while (true)
            {
                var eventsRange = boxEventsReader.TryCreateEventSeriesRange(exclusiveStartEventPointer, inclusiveEndTimestamp);
                var batch = boxEventsReader.ReadEvents(eventsRange, batchSize, x => x);
                if (batch.Any())
                    yield return batch;
                if (batch.Count < batchSize)
                    break;
                exclusiveStartEventPointer = new AllBoxEventSeriesPointer(batch.Last());
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
            var exclusiveStartEventPointer = new AllBoxEventSeriesPointer(firstBatch.Last());
            foreach (var batch in boxEventsReader.ReadEventsToEnd(exclusiveStartEventPointer, batchSize))
                yield return batch;
        }

        private const int defaultBatchSize = 5000;
    }
}