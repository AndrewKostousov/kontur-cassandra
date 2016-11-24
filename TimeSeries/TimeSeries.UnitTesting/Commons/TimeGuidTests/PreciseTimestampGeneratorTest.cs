using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace CassandraTimeSeries.UnitTesting.Commons.TimeGuidTests
{
    [TestFixture]
    public class PreciseTimestampGeneratorTest
    {
        [Test]
        [Category("Manual")]
        public void Perf()
        {
            const int count = 10 * 1000 * 1000;
            var timestampGenerator = new PreciseTimestampGenerator(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100));
            var sw = Stopwatch.StartNew();
            for(var i = 0; i < count; i++)
                timestampGenerator.NowTicks();
            sw.Stop();
            Console.Out.WriteLine("PreciseTimestampGenerator.Now() took {0} ms to generate {1} timestamps", sw.ElapsedMilliseconds, count);
        }

        [Test]
        [Category("Nightly")]
        public void Collisions()
        {
            const int count = 10 * 1000 * 1000;
            var timestampGenerator = new PreciseTimestampGenerator(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100));
            var results = new HashSet<long>();
            for(var i = 0; i < count; i++)
                results.Add(timestampGenerator.NowTicks());
            Assert.That(results.Count, Is.EqualTo(count));
        }

        [Test]
        public void EnsureMicrosecondResolution()
        {
            const int count = 1000 * 1000;
            var timeSeries = Enumerable.Range(0, count).Select(x => PreciseTimestampGenerator.Instance.NowTicks()).ToArray();
            for(var i = 1; i < count; i++)
                Assert.That(timeSeries[i] - timeSeries[i - 1], Is.GreaterThanOrEqualTo(10));
        }
    }
}