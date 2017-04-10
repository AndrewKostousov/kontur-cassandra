using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using Commons;
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
            var readEvents = readers.ToDictionary(r => r, r => new List<Event>());

            var keepReadersAlive = true;

            var readersThreads = readers.Select((reader, index) =>
                {
                    return new Thread(() =>
                    {
                        readEvents[reader].AddRange(reader.ReadFirst());

                        while (keepReadersAlive)
                            readEvents[reader].AddRange(reader.ReadNext());

                    }) { Name = $"reader #{index}" };
                }
            ).ToList();

            var writersThreads = writers.Select((writer, index) =>
            {
                return new Thread(() =>
                {
                    for (var i = 0; i < 20; ++i)
                    {
                        var eventProtos = Enumerable.Range(0, 10).Select(_ => new EventProto()).ToArray();
                        var timestamp = writer.WriteNext(eventProtos);

                        writtenEvents[writer].AddRange(timestamp.Select((t, n) => Tuple.Create(t, eventProtos[n])));
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

            if (shouldFail)
                AssertNotAllEventsWereRead(allWrittenEvents, readEvents.Values);
            else
                AssertAllEventsWereRead(allWrittenEvents, readEvents.Values);
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