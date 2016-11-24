using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Commons;
using Commons.Bits;
using Commons.TimeBasedUuid;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting.Commons.TimeGuidTests
{
    [TestFixture]
    public class TimeGuidGeneratorTest
    {
        [Test]
        [Category("Manual")]
        public void Perf()
        {
            const int count = 10 * 1000 * 1000;
            var guidGen = new TimeGuidGenerator(PreciseTimestampGenerator.Instance);
            var sw = Stopwatch.StartNew();
            for(var i = 0; i < count; i++)
                guidGen.NewGuid();
            sw.Stop();
            Console.Out.WriteLine("TimeGuidGenerator.NewGuid() took {0} ms to generate {1} time guids", sw.ElapsedMilliseconds, count);
        }

        [Test]
        [Category("Manual")]
        public void Collisions()
        {
            var guidGen = new TimeGuidGenerator(PreciseTimestampGenerator.Instance);
            var results = new Dictionary<byte[], byte>(10 * 1000 * 1000, ByteArrayComparer.Instance);
            for(var i = 0; i < 10 * 1000 * 1000; i++)
                results.Add(guidGen.NewGuid(), 0);
        }

        [Test]
        [Category("Manual")]
        public void Collisions_MultiProc()
        {
            const int count = 1000 * 1000;
            const int threadsCount = 50;
            var lists = new List<List<byte[]>>();
            var threads = new List<Thread>();
            var startSignal = new ManualResetEvent(false);
            for(var i = 0; i < threadsCount; i++)
            {
                var list = new List<byte[]>();
                lists.Add(list);
                var thread = new Thread(() =>
                    {
                        startSignal.WaitOne();
                        var guidGen = new TimeGuidGenerator(new PreciseTimestampGenerator(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100)));
                        for(var i1 = 0; i1 < count; i1++)
                            list.Add(guidGen.NewGuid());
                    });
                thread.Start();
                threads.Add(thread);
            }
            startSignal.Set();
            threads.ForEach(thread => thread.Join());
            Assert.That(lists.SelectMany(list => list).ToArray().Distinct(ByteArrayComparer.Instance).Count(), Is.EqualTo(threadsCount * count));
        }

        [Test]
        public void GenerateByTimestamp()
        {
            var guidGen = new TimeGuidGenerator(PreciseTimestampGenerator.Instance);
            var ts = Timestamp.Now;
            var guid = guidGen.NewGuid(ts);
            Assert.That(TimeGuidBitsLayout.GetTimestamp(guid), Is.EqualTo(ts));
        }

        [Test]
        public void GenerateByTimestamp_InvalidTimestamp()
        {
            var guidGen = new TimeGuidGenerator(PreciseTimestampGenerator.Instance);
            Assert.Throws<InvalidProgramStateException>(() => guidGen.NewGuid(Timestamp.MinValue));
            Assert.Throws<InvalidProgramStateException>(() => guidGen.NewGuid(Timestamp.MaxValue));
        }

        [Test]
        public void GenerateByTimestampAndClockSequence()
        {
            var guidGen = new TimeGuidGenerator(PreciseTimestampGenerator.Instance);
            var ts = Timestamp.Now;
            var clockSequence = new Random().NextUshort(TimeGuidBitsLayout.MinClockSequence, TimeGuidBitsLayout.MaxClockSequence + 1);
            var guid = guidGen.NewGuid(ts, clockSequence);
            Assert.That(TimeGuidBitsLayout.GetTimestamp(guid), Is.EqualTo(ts));
            Assert.That(TimeGuidBitsLayout.GetClockSequence(guid), Is.EqualTo(clockSequence));
        }

        [Test]
        public void GenerateByTimestampAndClockSequence_InvalidClockSequence()
        {
            var guidGen = new TimeGuidGenerator(PreciseTimestampGenerator.Instance);
            Assert.Throws<InvalidProgramStateException>(() => guidGen.NewGuid(Timestamp.Now, ushort.MaxValue));
            Assert.Throws<InvalidProgramStateException>(() => guidGen.NewGuid(Timestamp.Now, TimeGuidBitsLayout.MaxClockSequence + 1));
        }

        [Test]
        [Category("Manual")]
        public void RngTestPerf()
        {
            DoRngPerfTest("GenerateRandomNode()", GenerateRandomNode);
            DoRngPerfTest("GenerateRandomNodeCrypto()", GenerateRandomNodeCrypto);
        }

        private static void DoRngPerfTest(string actionName, Func<byte[]> generateRnadomNode)
        {
            var bytesGenerated = 0L;
            var sw = Stopwatch.StartNew();
            for(var i = 0; i < 1 * 1000 * 1000; i++)
                bytesGenerated += generateRnadomNode().Length;
            sw.Stop();
            Console.Out.WriteLine("{0} took {1} ms to generate {2} bytes", actionName, sw.ElapsedMilliseconds, bytesGenerated);
        }

        private byte[] GenerateRandomNode()
        {
            lock(rng)
                return rng.NextBytes(6);
        }

        private byte[] GenerateRandomNodeCrypto()
        {
            var bytes = new byte[6];
            cryptoRng.GetBytes(bytes);
            return bytes;
        }

        private readonly Random rng = new Random(Guid.NewGuid().GetHashCode());
        private readonly RNGCryptoServiceProvider cryptoRng = new RNGCryptoServiceProvider();
    }
}