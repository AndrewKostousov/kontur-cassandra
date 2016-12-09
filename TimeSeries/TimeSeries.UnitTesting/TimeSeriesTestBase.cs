using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;
using FluentAssertions;

namespace CassandraTimeSeries.UnitTesting
{
    public abstract class TimeSeriesTestBase
    {
        #region *** SetUp ***

        private static DatabaseWrapper wrapper;

        protected static ITimeSeries Series { get; private set; }

        [OneTimeSetUp]
        public void StartSession()
        {
            wrapper = new DatabaseWrapper("test");
            Series = new TimeSeries(wrapper.Table);
        }

        [OneTimeTearDown]
        public void DisposeSession()
        {
            wrapper.Dispose();
        }

        [SetUp]
        public void TruncateTable()
        {
            wrapper.Table.Truncate();
        }
        #endregion

        #region *** Test Templates ***
        protected void RunTest(
            IEnumerable<Event> eventsToWrite,
            Func<Timestamp, Timestamp, List<Event>> read)
        {
            RunTest(
                eventsToWrite,
                read,
                actual => actual.ShouldBeEquivalentTo(eventsToWrite, o => o.WithStrictOrderingFor(x => x))
                
                //CollectionAssert.AreEquivalent(eventsToWrite, actual)
            );
        }

        protected void RunTest(
            IEnumerable<Event> eventsToWrite,
            Func<Timestamp, Timestamp, List<Event>> read,
            Action<List<Event>> assert)
        {
            var start = Timestamp.Now - TimeSpan.FromMinutes(1);

            foreach (var e in eventsToWrite)
                Series.Write(e);

            var end = Timestamp.Now + TimeSpan.FromMinutes(1);

            var retrievedEvents = read(start, end);
            assert(retrievedEvents);
        }
        #endregion

        protected List<Event> CreateEvents(int count)
        {
            PreciseTimestampGenerator.Instance.NowTicks();
            
            return Enumerable.Range(0, count)
                .Select(i => new Event(Timestamp.Now, new [] {(byte)i}))
                .ToList();
        }
    }
}
