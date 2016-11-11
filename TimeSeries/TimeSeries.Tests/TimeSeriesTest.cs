using Cassandra;
using Cassandra.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CassandraTimeSeries.Tests
{
    [TestClass]
    public class TimeSeriesTest
    {
        static Cluster cluster = Cluster
            .Builder()
            .AddContactPoint("localhost")
            .Build();

        TimeSeries series = new TimeSeries(cluster, "test", "time_series", TimeSpan.FromMinutes(1));

        [TestInitialize]
        public void DropDatabase()
        {
            series.Database.DropTable();
            series = new TimeSeries(cluster, "test", "time_series", TimeSpan.FromMinutes(1));
        }

        [TestMethod]
        public void Series_CanWriteAndReadSingleEvent()
        {
            var ev = new Event();

            series.Write(ev);
            var retr = series.ReadRange(ev.Timestamp, ev.Timestamp + TimeSpan.FromMinutes(10)).Single();

            Assert.AreEqual(ev.Id, retr.Id);
        }

        [TestMethod]
        public void Series_CanWriteAndReadSeriesOfEvents()
        {
            var begin = DateTimeOffset.UtcNow;

            series.ParallelWrite(Enumerable.Range(0, 10).Select(_ => new Event()));

            var end = DateTimeOffset.UtcNow;
            
            var retr = series.ReadRange(begin, end);
            Assert.AreEqual(10, retr.Count());

            //todo Assert Events
        }

        [TestMethod]
        public void Series_ShouldNotReadWrongEvents_IfBeforeRange()
        {
            var begin = DateTimeOffset.UtcNow;

            series.ParallelWrite(Enumerable.Range(0, 10).Select(_ => new Event()));

            var end = DateTimeOffset.UtcNow;

            series.Write(new Event(begin - TimeSpan.FromMinutes(10), 4));

            var retr = series.ReadRange(begin, end);
            Assert.AreEqual(10, retr.Count());
        }

        [TestMethod]
        public void Series_ShouldNotReadWrongEvents_IfAfterRange()
        {
            var begin = DateTimeOffset.UtcNow;

            series.ParallelWrite(Enumerable.Range(0, 10).Select(_ => new Event()));

            var end = DateTimeOffset.UtcNow;

            series.Write(new Event(end + TimeSpan.FromMinutes(10), 4));

            var retr = series.ReadRange(begin, end);
            Assert.AreEqual(10, retr.Count());
        }

        [TestMethod]
        public void Series_CanReadAndWrite_IfDifferentPayloadLength()
        {
            var begin = DateTimeOffset.UtcNow;

            series.ParallelWrite(Enumerable.Range(0, 10).Select(x => new Event(x)));

            var end = DateTimeOffset.UtcNow;

            var retr = series.ReadRange(begin, end);
            Assert.AreEqual(10, retr.Count());
        }
    }
}
