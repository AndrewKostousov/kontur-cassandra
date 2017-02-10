using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using CassandraTimeSeries.Utils;
using Commons.TimeBasedUuid;
using FluentAssertions;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    public abstract class CommonTimeSeriesTestParallel : TimeSeriesTestBase
    {
        [SetUp]
        public void TruncateTable()
        {
            Wrapper.Table.Truncate();
        }

        [Test]
        public void ReadAndWrite_InParallel_SingleReader_SingleWriter()
        {
            DoTestParallel(readersCount: 1, writersCount: 1);
        }

        [Test]
        public void ReadAndWrite_InParallel_SingleReader_ManyWriters()
        {
            DoTestParallel(readersCount: 1, writersCount: 4);
        }

        [Test]
        public void ReadAndWrite_InParallel_ManyReaders_SingleWriter()
        {
            DoTestParallel(readersCount: 4, writersCount: 1);
        }

        [Test]
        public void ReadAndWrite_InParallel_ManyReaders_ManyWriters()
        {
            DoTestParallel(readersCount: 4, writersCount: 4);
        }

        private void DoTestParallel(int readersCount, int writersCount)
        {
            var readers = Enumerable.Range(0, readersCount).Select(_ => new EventReader(TimeSeriesFactory(), new ReaderSettings())).ToList();
            var writers = Enumerable.Range(0, writersCount).Select(_ => new EventWriter(TimeSeriesFactory(), new WriterSettings())).ToList();

            var writtenEvents = writers.ToDictionary(r => r, r => new List<Event>());
            var readEvents = readers.ToDictionary(r => r, r => new List<Event>());
            
            var keepReadersAlive = true;

            var readersThreads = readers.Select(reader =>
                {
                    return new Thread(() =>
                    {
                        readEvents[reader].Add(reader.ReadFirst());

                        while (keepReadersAlive)
                            readEvents[reader].AddRange(reader.ReadNext());
                    });
                }
            ).ToList();

            var writersThreads = writers.Select(writer =>
            {
                return new Thread(() =>
                {
                    for (int i = 0; i < 100; ++i)
                        writtenEvents[writer].Add(writer.WriteNext());
                });
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

            var allWrittenEvents = writtenEvents
                .SelectMany(x => x.Value)
                .OrderBy(x => x.Id)
                .ToList();

            foreach (var eventsReadBySingleReader in readEvents.Values)
                eventsReadBySingleReader.ShouldBeExactly(allWrittenEvents);
        }
    }
}