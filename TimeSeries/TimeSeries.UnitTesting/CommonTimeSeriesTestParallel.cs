using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;
using FluentAssertions;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    public abstract class CommonTimeSeriesTestParallel<TDatabaseController> : TimeSeriesTestBase<TDatabaseController> 
        where TDatabaseController : IDatabaseController, new()
    {
        protected virtual bool ShouldFailWithManyWriters { get; } = false;

        [SetUp]
        public void ResetDatabaseSchema()
        {
            Database.ResetSchema();
        }

        [Test]
        public void ReadAndWrite_InParallel_SingleReader_SingleWriter()
        {
            DoTestParallel(readersCount: 1, writersCount: 1, shouldFail: false);
        }

        [Test]
        public void ReadAndWrite_InParallel_SingleReader_ManyWriters()
        {
            DoTestParallel(readersCount: 1, writersCount: 4, shouldFail: ShouldFailWithManyWriters);
        }

        [Test]
        public void ReadAndWrite_InParallel_ManyReaders_SingleWriter()
        {
            DoTestParallel(readersCount: 4, writersCount: 1, shouldFail: false);
        }

        [Test]
        public void ReadAndWrite_InParallel_ManyReaders_ManyWriters()
        {
            DoTestParallel(readersCount: 4, writersCount: 4, shouldFail: ShouldFailWithManyWriters);
        }

        private class ReadStamp
        {
            private readonly Timestamp timestamp;
            private readonly long partitionId;

            public Event[] Events { get; }

            private static readonly PreciseTimestampGenerator time = PreciseTimestampGenerator.Instance;
            private static readonly TimeLinePartitioner partitioner = new TimeLinePartitioner();

            public ReadStamp(Event[] events)
            {
                Events = events;
                timestamp = new Timestamp(time.NowTicks());
                partitionId = events.Length > 0 ? partitioner.CreatePartitionId(events[0].TimeGuid.GetTimestamp()) : 0;
            }

            public override string ToString()
            {
                return Events.Length.ToString();
            }
        }

        private void DoTestParallel(int readersCount, int writersCount, bool shouldFail)
        {
            var readers =
                Enumerable.Range(0, readersCount)
                    .Select(_ => new EventReader(TimeSeriesFactory(new TDatabaseController()), new ReaderSettings()))
                    .ToList();
            var writers =
                Enumerable.Range(0, writersCount)
                    .Select(_ => new EventWriter(TimeSeriesFactory(new TDatabaseController()), new WriterSettings()))
                    .ToList();

            var writtenEvents = writers.ToDictionary(r => r, r => new List<Tuple<Timestamp, EventProto>>());
            var readEvents = readers.ToDictionary(r => r, r => new List<ReadStamp>());

            var keepReadersAlive = true;

            var readersThreads = readers.Select((reader, index) =>
                {
                    return new Thread(() =>
                    {
                        readEvents[reader].Add(new ReadStamp(reader.ReadFirst()));

                        while (keepReadersAlive)
                            readEvents[reader].Add(new ReadStamp(reader.ReadNext()));

                    }) { Name = $"reader #{index}" };
                }
            ).ToList();

            var writersThreads = writers.Select((writer, index) =>
            {
                return new Thread(() =>
                {
                    for (var i = 0; i < 100; ++i)
                    {
                        var eventProto = new EventProto();
                        var timestamp = writer.WriteNext(eventProto);
                        writtenEvents[writer].Add(Tuple.Create(timestamp[0], eventProto));
                    }
                }) {Name = $"writer #{index}"};
            }).ToList();

            foreach (var writer in writersThreads)
                writer.Start();

            foreach (var reader in readersThreads)
                reader.Start();

            foreach (var writer in writersThreads)
                writer.Join();

            Thread.Sleep(1000); // wait readers

            keepReadersAlive = false;

            foreach (var reader in readersThreads)
                reader.Join();

            var allWrittenEvents = writtenEvents.SelectMany(x => x.Value).OrderBy(x => x.Item1.Ticks).ToList();
            var allReadEvents = readEvents.Values.Select(x => x.SelectMany(z => z.Events).ToList()).ToList();

            ValidateReads(readEvents.Values.ToList());

            if (shouldFail)
                AssertNotAllEventsWereRead(allWrittenEvents, allReadEvents);
            else
                AssertAllEventsWereRead(allWrittenEvents, allReadEvents);
        }

        private void ValidateReads(List<List<ReadStamp>> reads)
        {
            var readByThread = reads.Select(x => x.SelectMany(z => z.Events).ToList()).ToList();

            for (int i = 0; i < reads[0].Count; ++i)
            { 
                foreach (var read in readByThread)
                    foreach (var read2 in readByThread)
                        if (i < read.Count && i < read2.Count && read2[i].TimeGuid != read[i].TimeGuid)
                        {
                            int[] indicesFirst = new int[reads.Count];
                            int[] indicesSecond = new int[reads.Count];

                            for (int j = 0; j < reads.Count; ++j)
                            {
                                var firstOrDefault = reads[j]
                                    .Select((r, k) => Tuple.Create(r, k))
                                    .FirstOrDefault(t => t.Item1.Events.Select(z => z.TimeGuid).Contains(read[i].TimeGuid));

                                indicesFirst[j] = firstOrDefault?.Item2 ?? -1;

                                firstOrDefault = reads[j]
                                    .Select((r, k) => Tuple.Create(r, k))
                                    .FirstOrDefault(t => t.Item1.Events.Select(z => z.TimeGuid).Contains(read2[i].TimeGuid));

                                indicesSecond[j] = firstOrDefault?.Item2 ?? -1;
                            }

                            throw new Exception();
                        }
                }
        }

        private void AssertNotAllEventsWereRead(List<Tuple<Timestamp, EventProto>> allWrittenEvents, IEnumerable<List<Event>> readByThread)
        {
            var allReadIds = readByThread
                .Select(r => new HashSet<Guid>(r.Select(x => x.Proto.UserId)))
                .ToList();

            var eventsNotReadByReaders = allWrittenEvents
                .Where(x => allReadIds.Any(s => !s.Contains(x.Item2.UserId)));

            eventsNotReadByReaders.Should().NotBeEmpty();
        }

        private void AssertAllEventsWereRead(List<Tuple<Timestamp, EventProto>> allWrittenEvents, IEnumerable<List<Event>> readByThread)
        {
            foreach (var eventsReadBySingleReader in readByThread)
            {
                eventsReadBySingleReader.Select(x => x.Proto).ShouldBeExactly(allWrittenEvents.Select(x => x.Item2));
                eventsReadBySingleReader.Select(x => x.Timestamp).ShouldBeExactly(allWrittenEvents.Select(x => x.Item1));
            }
        }
    }
}