using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Commons;
using Commons.Logging;
using Commons.Testing;
using FluentAssertions;
using JetBrains.Annotations;
using MoreLinq;
using NUnit.Framework;

namespace EdiTimeline.Tests
{
    public class AllBoxEventSeries_Write_Test : BoxEventSeriesTestBase
    {
        [Test]
        public void Write_Batch()
        {
            var firstEvent = WriteToAllBoxEventSeries(ProtoBoxEvent(0xff)).Single();
            var expectedEvents = WriteToAllBoxEventSeries(Enumerable.Range(0, 10).Select(x => ProtoBoxEvent((byte)x)).ToArray());
            var actualEvents = ReadEventsToEnd(firstEvent);
            actualEvents.ShouldBeEquivalentWithOrderTo(expectedEvents);
            for (var i = 0; i < expectedEvents.Count; i++)
            {
                actualEvents = ReadEventsToEnd(expectedEvents[i]);
                actualEvents.ShouldBeEquivalentWithOrderTo(expectedEvents.Skip(i + 1).ToList());
            }
        }

        [Test]
        public void Write_ManyEvents()
        {
            var firstEvent = WriteToAllBoxEventSeries(ProtoBoxEvent(0xff)).Single();

            var sw = Stopwatch.StartNew();
            var expectedEvents = WriteToAllBoxEventSeries(Enumerable.Range(0, 10000).Select(x => ProtoBoxEvent()).ToArray());
            sw.Stop();
            Log.For(this).Information("Write({0}) took {1} ms", expectedEvents.Count, sw.ElapsedMilliseconds);

            sw = Stopwatch.StartNew();
            var actualEvents = ReadEventsToEnd(firstEvent);
            sw.Stop();
            Log.For(this).Information("ReadEventsToEnd({0}) took {1} ms", actualEvents.Count, sw.ElapsedMilliseconds);

            actualEvents.ShouldBeEquivalentWithOrderTo(expectedEvents);
        }

        [TestCaseSource(nameof(concurrentTestCases))]
        public void ReadWrite_Concurrent(AllBoxEventSeriesSettingsForTests settings)
        {
            allBoxEventSeries = new AllBoxEventSeries(settings, serializer, allBoxEventSeriesTicksHolder, cassandraCluster);

            var firstEvent = WriteToAllBoxEventSeries(ProtoBoxEvent(0xff)).Single();

            const int writerThreads = 8;
            const int readerThreads = 4;
            var sharedState = new SharedState(writerThreads, readerThreads, eventsPerWriterThread: 5000);
            var actions = new List<Action<SharedState>>();
            for (var th = 0; th < writerThreads; th++)
            {
                var writerThreadIndex = th;
                actions.Add(state =>
                {
                    const int writingBatchSize = 10;
                    var rng = new Random(Guid.NewGuid().GetHashCode());
                    foreach (var batch in state.ProtoEventsToWrite[writerThreadIndex].Batch(writingBatchSize, Enumerable.ToArray))
                    {
                        var protoEventsToWrite = batch;
                        while (protoEventsToWrite.Any())
                        {
                            var queueItems = protoEventsToWrite.Select(x => new AllBoxEventSeriesWriterQueueItem(x, new Promise<Timestamp>())).ToList();
                            allBoxEventSeries.WriteEventsInAnyOrder(queueItems);
                            var boxEvents = queueItems.Where(x => x.EventTimestamp.Result != null)
                                                      .Select(x => new BoxEvent(x.ProtoBoxEvent.EventId, x.EventTimestamp.Result, x.ProtoBoxEvent.Payload))
                                                      .ToList();
                            state.WrittenEvents[writerThreadIndex].AddRange(boxEvents);
                            protoEventsToWrite = queueItems.Where(x => x.EventTimestamp.Result == null).Select(x => x.ProtoBoxEvent).ToArray();
                        }
                        Thread.Sleep(TimeSpan.FromMilliseconds(rng.Next(30)));
                    }
                });
            }
            for (var th = 0; th < readerThreads; th++)
            {
                var readerThreadIndex = th;
                actions.Add(state =>
                {
                    var rng = new Random(Guid.NewGuid().GetHashCode());
                    var readEvents = state.ReadEvents[readerThreadIndex];
                    do
                    {
                        var exclusiveStartEvent = readEvents.LastOrDefault() ?? firstEvent;
                        readEvents.AddRange(ReadEventsToEnd(exclusiveStartEvent));
                        Thread.Sleep(TimeSpan.FromMilliseconds(rng.Next(30)));
                    } while (readEvents.Count < state.TotalEventsToRead);
                });
            }

            var sw = Stopwatch.StartNew();
            MultithreadingTestHelper.RunOnSeparateThreads(sharedState, TimeSpan.FromMinutes(5), actions);
            sw.Stop();
            Log.For(this).Information("ConcurrentReadWrite({0}) took {1} ms", sharedState.TotalEventsToRead, sw.ElapsedMilliseconds);

            for (var th = 0; th < sharedState.WrittenEvents.Length; th++)
            {
                var actualEvents = sharedState.WrittenEvents[th];
                var expectedProtoBoxEvents = sharedState.ProtoEventsToWrite[th];
                actualEvents.Select(x => new ProtoBoxEvent(x.EventId, x.Payload)).ShouldBeEquivalentTo(expectedProtoBoxEvents);
            }
            foreach (var actualEvents in sharedState.ReadEvents)
            {
                var expectedEvents = sharedState.WrittenEvents.SelectMany(x => x).OrderBy(x => x.EventTimestamp).ThenBy(x => x.EventId.ToString()).ToList();
                // note: actualEvents.ShouldBeEquivalentWithOrderTo(expectedEvents) became extremely slow because of a call to ShouldBeEquivalentTo() inside opts.Using()
                Assert.That(actualEvents.Count, Is.EqualTo(expectedEvents.Count));
                for (var i = 0; i < actualEvents.Count; i++)
                    actualEvents[i].ShouldBeEquivalentTo(expectedEvents[i]);
            }
        }

        [NotNull]
        private List<BoxEvent> ReadEventsToEnd([NotNull] BoxEvent exclusiveStartEvent)
        {
            var range = allBoxEventSeries.TryCreateRange(new AllBoxEventSeriesPointer(exclusiveStartEvent.EventTimestamp, exclusiveStartEvent.EventId), inclusiveEndTimestamp: null);
            range.Should().NotBeNull();
            return allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x);
        }

        private static readonly AllBoxEventSeriesSettingsForTests[] concurrentTestCases =
            {
                new AllBoxEventSeriesSettingsForTests(TimeSpan.FromMilliseconds(100), minBatchSizeForRead: 3),
                new AllBoxEventSeriesSettingsForTests(TimeSpan.FromSeconds(1), minBatchSizeForRead: 10),
                new AllBoxEventSeriesSettingsForTests(TimeSpan.FromMinutes(10), minBatchSizeForRead: 100),
            };

        private class SharedState : MultithreadingTestHelper.SharedState
        {
            public SharedState(int writerThreads, int readerThreads, int eventsPerWriterThread)
            {
                TotalEventsToRead = writerThreads * eventsPerWriterThread;
                ProtoEventsToWrite = Enumerable.Range(0, writerThreads)
                                               .Select(x => Enumerable.Range(0, eventsPerWriterThread).Select(y => ProtoBoxEvent()).ToArray())
                                               .ToArray();
                ReadEvents = Enumerable.Range(0, readerThreads).Select(x => new List<BoxEvent>()).ToArray();
                WrittenEvents = Enumerable.Range(0, writerThreads).Select(x => new List<BoxEvent>()).ToArray();
            }

            public int TotalEventsToRead { get; }
            public ProtoBoxEvent[][] ProtoEventsToWrite { get; }
            public List<BoxEvent>[] WrittenEvents { get; }
            public List<BoxEvent>[] ReadEvents { get; }
        }
    }
}