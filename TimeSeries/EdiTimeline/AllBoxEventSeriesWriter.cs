using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Commons;
using Commons.Logging;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class AllBoxEventSeriesWriter : IDisposable
    {
        public AllBoxEventSeriesWriter(IAllBoxEventSeries allBoxEventSeries)
        {
            this.allBoxEventSeries = allBoxEventSeries;
            thread = new Thread(QueueRaker)
                {
                    Name = "AllBoxEventSeriesWriter",
                    IsBackground = true,
                };
            thread.Start();
        }

        public void Dispose()
        {
            if (disposeInitiated)
                return;
            disposeInitiated = true;
            lock (locker)
            {
                var batch = queue.ToList();
                foreach (var entry in batch)
                    entry.EventTimestamp.SetResult(null);
                queue.Clear();
            }
            @event.Set();
            thread.Join();
            @event.Dispose();
        }

        [NotNull]
        public Timestamp Write([NotNull] ProtoBoxEvent protoBoxEvent)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                const int maxAttemptsCount = 10;
                var sleepDuration = TimeSpan.FromMilliseconds(10);
                for (var attempt = 0; attempt < maxAttemptsCount; attempt++)
                {
                    var eventTimestampPromise = TryEnqueue(protoBoxEvent);
                    if (eventTimestampPromise == null)
                        break;
                    var eventTimestamp = eventTimestampPromise.Result;
                    if (eventTimestamp != null)
                        return eventTimestamp;
                    if (attempt > 0)
                        sleepDuration = sleepDuration.Multiply(attempt % 2 == 1 ? 2 : 5); // wait for 10ms, 20ms, 100ms, 200ms, 1000ms, etc.
                    Thread.Sleep(sleepDuration);
                }
                throw new InvalidProgramStateException($"Failed to write event in {maxAttemptsCount} attempts: {protoBoxEvent}");
            }
            finally
            {
                sw.Stop();
                if (sw.Elapsed >= TimeSpan.FromSeconds(10))
                    Log.For(this).Warning("[LONG DELAY] AllBoxEventSeriesWriter.Write() took {elapsed} for: {@protoBoxEvent}", sw.Elapsed, protoBoxEvent);
            }
        }

        [CanBeNull]
        private Promise<Timestamp> TryEnqueue([NotNull] ProtoBoxEvent protoBoxEvent)
        {
            lock (locker)
            {
                if (disposeInitiated)
                    return null;
                var eventTimestamp = new Promise<Timestamp>();
                queue.Enqueue(new AllBoxEventSeriesWriterQueueItem(protoBoxEvent, eventTimestamp));
                if (queue.Count * runs >= sum && queue.Count >= 10)
                    @event.Set();
                return eventTimestamp;
            }
        }

        private void QueueRaker()
        {
            while (!disposeInitiated)
            {
                @event.WaitOne(TimeSpan.FromMilliseconds(10));
                var queueItems = RakeBatch();
                @event.Reset();
                if (queueItems.Count == 0)
                    continue;
                ++runs;
                sum += queueItems.Count;
                allBoxEventSeries.WriteEventsInAnyOrder(queueItems);
            }
        }

        [NotNull]
        private List<AllBoxEventSeriesWriterQueueItem> RakeBatch()
        {
            lock (locker)
            {
                var result = queue.ToList();
                queue.Clear();
                return result;
            }
        }

        private volatile bool disposeInitiated;
        private volatile float sum;
        private volatile float runs;
        private readonly Thread thread;
        private readonly IAllBoxEventSeries allBoxEventSeries;
        private readonly object locker = new object();
        private readonly ManualResetEvent @event = new ManualResetEvent(false);
        private readonly Queue<AllBoxEventSeriesWriterQueueItem> queue = new Queue<AllBoxEventSeriesWriterQueueItem>();
    }
}