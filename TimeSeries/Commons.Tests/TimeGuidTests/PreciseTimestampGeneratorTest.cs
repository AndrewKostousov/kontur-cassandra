using System;
using System.Collections.Generic;
using System.Linq;
using Commons.TimeBasedUuid;
using NUnit.Framework;

namespace Commons.Tests.TimeGuidTests
{
    [TestFixture]
    public class PreciseTimestampGeneratorTest
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(16)]
        [TestCase(32)]
        [TestCase(64)]
        [TestCase(128)]
        [Category("Manual")]
        public void Perf(int threadsCount)
        {
            const int totalIterationsCount = 64 * 1000 * 1000;
            var sut = new PreciseTimestampGenerator(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100));
            PerfMeasurement.Do("PreciseTimestampGenerator.Now()", threadsCount, totalIterationsCount, () => sut.NowTicks());
        }

        [Test]
        [Category("Nightly")]
        public void Collisions()
        {
            const int count = 32 * 1000 * 1000;
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