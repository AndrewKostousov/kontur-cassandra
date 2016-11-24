using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Commons;
using Commons.Bits;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting.Commons.BitsTests
{
    [TestFixture]
    public class BitHelperTest
    {
        [SetUp]
        public void SetUp()
        {
            random = new Random((int)(DateTime.UtcNow.Ticks % ((long)1 << 32)));
        }

        [Test]
        public void Byte_ToBytes_ToByte()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                Byte_ToBytes_ToByte(random.NextByte());
            Byte_ToBytes_ToByte(byte.MinValue);
            Byte_ToBytes_ToByte(byte.MaxValue);
        }

        private static void Byte_ToBytes_ToByte(byte value)
        {
            var offset = 0;
            var buffer = new byte[1];
            BitHelper.ByteToBytes(value, buffer, ref offset);
            offset = 0;
            Assert.That(BitHelper.ReadByte(buffer, ref offset), Is.EqualTo(value));
        }

        [Test]
        public void Uint_ToBytes_ToUint()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                Uint_ToBytes_ToUint(random.NextUint());
            Uint_ToBytes_ToUint(uint.MinValue);
            Uint_ToBytes_ToUint(uint.MaxValue);
        }

        private static void Uint_ToBytes_ToUint(uint value)
        {
            var offset = 0;
            var bytes1 = new byte[4];
            BitHelper.UintToBytes(value, bytes1, ref offset);
            var bytes2 = BitHelper.UintToBytes(value);
            offset = 0;
            Assert.That(BitHelper.ReadUint(bytes1, ref offset), Is.EqualTo(value));
            Assert.That(BitHelper.ReadUint(bytes2), Is.EqualTo(value));
        }

        [Test]
        public void UintToBytesFast_Correctness()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                UintToBytesFast_Correctness(random.NextUint());
            UintToBytesFast_Correctness(uint.MinValue);
            UintToBytesFast_Correctness(uint.MaxValue);
        }

        private static void UintToBytesFast_Correctness(uint value)
        {
            var offset = 0;
            var buffer = new byte[4];
            BitHelper.UintToBytes(value, buffer, ref offset);
            var expectedBytes = BitConverter.GetBytes(value).Reverse().ToArray();
            Assert.That(ByteArrayComparer.Instance.Equals(expectedBytes, buffer), Is.True);
        }

        [Test]
        public void Ushort_ToBytes_ToUshort()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                Ushort_ToBytes_ToUshort(random.NextUshort());
            Ushort_ToBytes_ToUshort(ushort.MinValue);
            Ushort_ToBytes_ToUshort(ushort.MaxValue);
        }

        private static void Ushort_ToBytes_ToUshort(ushort value)
        {
            var offset = 0;
            var bytes1 = new byte[2];
            BitHelper.UshortToBytes(value, bytes1, ref offset);
            var bytes2 = BitHelper.UshortToBytes(value);
            offset = 0;
            Assert.That(BitHelper.ReadUshort(bytes1, ref offset), Is.EqualTo(value));
            Assert.That(BitHelper.ReadUshort(bytes2), Is.EqualTo(value));
        }

        [Test]
        public void UshortToBytesFast_Correctness()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                UshortToBytesFast_Correctness(random.NextUshort());
            UshortToBytesFast_Correctness(ushort.MinValue);
            UshortToBytesFast_Correctness(ushort.MaxValue);
        }

        private static void UshortToBytesFast_Correctness(ushort value)
        {
            var offset = 0;
            var buffer = new byte[2];
            BitHelper.UshortToBytes(value, buffer, ref offset);
            var expectedBytes = BitConverter.GetBytes(value).Reverse().ToArray();
            Assert.That(ByteArrayComparer.Instance.Equals(expectedBytes, buffer), Is.True);
        }

        [Test]
        public void Long_ToBytes_ToLong()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                Long_ToBytes_ToLong(random.NextLong());
            Long_ToBytes_ToLong(long.MinValue);
            Long_ToBytes_ToLong(long.MaxValue);
        }

        private static void Long_ToBytes_ToLong(long value)
        {
            var offset = 0;
            var bytes2 = new byte[8];
            BitHelper.LongToBytes(value, bytes2, ref offset);
            var bytes1 = BitHelper.LongToBytes(value);
            offset = 0;
            Assert.That(BitHelper.ReadLong(bytes2, ref offset), Is.EqualTo(value));
            Assert.That(BitHelper.ReadLong(bytes1), Is.EqualTo(value));
        }

        [Test]
        public void LongToBytesFast_Correctness()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                LongToBytesFast_Correctness(random.NextLong());
            LongToBytesFast_Correctness(long.MinValue);
            LongToBytesFast_Correctness(long.MaxValue);
        }

        private static void LongToBytesFast_Correctness(long value)
        {
            var offset = 0;
            var buffer = new byte[8];
            BitHelper.LongToBytes(value, buffer, ref offset);
            var expectedBytes = BitConverter.GetBytes(value).Reverse().ToArray();
            Assert.That(ByteArrayComparer.Instance.Equals(expectedBytes, buffer), Is.True);
        }

        [Test]
        public void Ulong_ToBytes_ToUlong()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                Ulong_ToBytes_ToUlong(random.NextUlong());
            Ulong_ToBytes_ToUlong(ulong.MinValue);
            Ulong_ToBytes_ToUlong(ulong.MaxValue);
        }

        private static void Ulong_ToBytes_ToUlong(ulong value)
        {
            var offset = 0;
            var bytes1 = new byte[8];
            BitHelper.UlongToBytes(value, bytes1, ref offset);
            var bytes2 = BitHelper.UlongToBytes(value);
            offset = 0;
            Assert.That(BitHelper.ReadUlong(bytes1, ref offset), Is.EqualTo(value));
            Assert.That(BitHelper.ReadUlong(bytes2), Is.EqualTo(value));
        }

        [Test]
        public void UlongToBytesFast_Correctness()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                UlongToBytesFast_Correctness(random.NextUlong());
            UlongToBytesFast_Correctness(ulong.MinValue);
            UlongToBytesFast_Correctness(ulong.MaxValue);
        }

        private static void UlongToBytesFast_Correctness(ulong value)
        {
            var offset = 0;
            var buffer = new byte[8];
            BitHelper.UlongToBytes(value, buffer, ref offset);
            var expectedBytes = BitConverter.GetBytes(value).Reverse().ToArray();
            Assert.That(ByteArrayComparer.Instance.Equals(expectedBytes, buffer), Is.True);
        }

        [Test]
        public void DateTime_ToBytes_ToDateTime()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                DateTime_ToBytes_ToDateTime(random.NextDateTime());
            DateTime_ToBytes_ToDateTime(DateTime.MinValue);
            DateTime_ToBytes_ToDateTime(DateTime.MaxValue);
        }

        private static void DateTime_ToBytes_ToDateTime(DateTime time)
        {
            var offset = 0;
            var buffer = new byte[8];
            BitHelper.DateTimeToBytes(time, buffer, ref offset);
            offset = 0;
            Assert.That(BitHelper.ReadDateTime(buffer, ref offset), Is.EqualTo(time));
        }

        [Test]
        public void DateTimeToBytesFast_Correctness()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                DateTimeToBytesFast_Correctness(random.NextDateTime());
            DateTimeToBytesFast_Correctness(DateTime.MinValue);
            DateTimeToBytesFast_Correctness(DateTime.MaxValue);
            DateTimeToBytesFast_Correctness(DateTime.Now);
            DateTimeToBytesFast_Correctness(DateTime.UtcNow);
        }

        private static void DateTimeToBytesFast_Correctness(DateTime value)
        {
            var offset = 0;
            var buffer = new byte[8];
            BitHelper.DateTimeToBytes(value, buffer, ref offset);
            var expectedBytes = BitConverter.GetBytes(value.Ticks).Reverse().ToArray();
            Assert.That(ByteArrayComparer.Instance.Equals(expectedBytes, buffer), Is.True);
        }

        [Test]
        public void Timestamp_ToBytes_ToTimestamp()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                Timestamp_ToBytes_ToTimestamp(random.NextTimestamp());
            Timestamp_ToBytes_ToTimestamp(Timestamp.MinValue);
            Timestamp_ToBytes_ToTimestamp(Timestamp.MaxValue);
        }

        private static void Timestamp_ToBytes_ToTimestamp(Timestamp timestamp)
        {
            var offset = 0;
            var bytes1 = new byte[8];
            BitHelper.TimestampToBytes(timestamp, bytes1, ref offset);
            var bytes2 = BitHelper.TimestampToBytes(timestamp);
            offset = 0;
            Assert.That(BitHelper.ReadTimestamp(bytes1, ref offset), Is.EqualTo(timestamp));
            Assert.That(BitHelper.ReadTimestamp(bytes2), Is.EqualTo(timestamp));
        }

        [Test]
        public void TimestampToBytesFast_Correctness()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                TimestampToBytesFast_Correctness(random.NextTimestamp());
            TimestampToBytesFast_Correctness(Timestamp.MinValue);
            TimestampToBytesFast_Correctness(Timestamp.MaxValue);
            TimestampToBytesFast_Correctness(Timestamp.Now);
        }

        private static void TimestampToBytesFast_Correctness(Timestamp value)
        {
            var offset = 0;
            var buffer = new byte[8];
            BitHelper.TimestampToBytes(value, buffer, ref offset);
            var expectedBytes = BitConverter.GetBytes(value.Ticks).Reverse().ToArray();
            Assert.That(ByteArrayComparer.Instance.Equals(expectedBytes, buffer), Is.True);
        }

        [Test]
        public void Timestamp_ToBytes_Order()
        {
            var t1 = new Timestamp(100500);
            var t2 = new Timestamp(100600);
            var offset = 0;
            var buffer1 = new byte[8];
            BitHelper.TimestampToBytes(t1, buffer1, ref offset);
            offset = 0;
            var buffer2 = new byte[8];
            BitHelper.TimestampToBytes(t2, buffer2, ref offset);
            Assert.That(ByteArrayComparer.Instance.Compare(buffer1, buffer2) < 0);
        }

        [Test]
        public void Timestamp_ToBytesReverse_Order()
        {
            var t1 = new Timestamp(100500);
            var t2 = new Timestamp(100600);
            var offset = 0;
            var buffer1 = new byte[8];
            BitHelper.TimestampToBytesReverse(t1, buffer1, ref offset);
            offset = 0;
            var buffer2 = new byte[8];
            BitHelper.TimestampToBytesReverse(t2, buffer2, ref offset);
            Assert.That(ByteArrayComparer.Instance.Compare(buffer1, buffer2) > 0);
        }

        [Test]
        public void Guid_Order()
        {
            var offset = 0;
            var buffer1 = new byte[16];
            var guid1 = new Guid("00000001-0000-0000-0000-000000000000");
            BitHelper.GuidToBytes(guid1, buffer1, ref offset);
            offset = 0;
            var buffer2 = new byte[16];
            var guid2 = new Guid("10000000-0000-0000-0000-000000000000");
            BitHelper.GuidToBytes(guid2, buffer2, ref offset);
            Assert.That(guid2, Is.GreaterThan(guid1).Using((IComparer)Comparer<Guid>.Default));
            Assert.That(buffer1, Is.GreaterThan(buffer2).Using(ByteArrayComparer.Instance));
        }

        [Test]
        public void Guid_ToBytes_ToGuid()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                Guid_ToBytes_ToGuid(random.NextGuid());
            Guid_ToBytes_ToGuid(GuidHelpers.MinGuid);
            Guid_ToBytes_ToGuid(GuidHelpers.MaxGuid);
        }

        private static void Guid_ToBytes_ToGuid(Guid guid)
        {
            var offset = 0;
            var buffer = new byte[16];
            BitHelper.GuidToBytes(guid, buffer, ref offset);
            offset = 0;
            Assert.That(BitHelper.ReadGuid(buffer, ref offset), Is.EqualTo(guid));
        }

        [Test]
        public void GuidToBytesFast_Correctness()
        {
            for(var i = 0; i < iterationsOfRandom; i++)
                GuidToBytesFast_Correctness(random.NextGuid());
            GuidToBytesFast_Correctness(GuidHelpers.MinGuid);
            GuidToBytesFast_Correctness(GuidHelpers.MaxGuid);
        }

        private static void GuidToBytesFast_Correctness(Guid value)
        {
            var offset = 0;
            var buffer = new byte[16];
            BitHelper.GuidToBytes(value, buffer, ref offset);
            var expectedBytes = value.ToByteArray();
            Assert.That(ByteArrayComparer.Instance.Equals(expectedBytes, buffer), Is.True);
        }

        [Test]
        [Category("Manual")]
        public void DateTime_ToBytes_Perf()
        {
            var dateTimes = new List<DateTime>();
            const int itersCount = 10000000;
            for(var i = 0; i < itersCount; i++)
                dateTimes.Add(random.NextDateTime());

            var sw = Stopwatch.StartNew();
            for(var i = 0; i < itersCount; i++)
            {
                var b = BitConverter.GetBytes(dateTimes[i].Ticks).Reverse().ToArray();
            }
            sw.Stop();
            Console.Out.WriteLine("FieldSerializer.DateTimeToBytes() took {0} ms", sw.ElapsedMilliseconds);

            var buffer = new byte[8];
            sw = Stopwatch.StartNew();
            for(var i = 0; i < itersCount; i++)
            {
                var offset = 0;
                BitHelper.DateTimeToBytes(dateTimes[i], buffer, ref offset);
            }
            sw.Stop();
            Console.Out.WriteLine("FieldSerializer.DateTimeToBytesFast() took {0} ms", sw.ElapsedMilliseconds);
        }

        [Test]
        [Category("Manual")]
        public void Endianess_Perf()
        {
            var longs = new List<long>();
            const int itersCount = 10000000;
            for(var i = 0; i < itersCount; i++)
                longs.Add(random.NextLong());

            var buffer = new byte[8];
            var sw = Stopwatch.StartNew();
            for(var i = 0; i < itersCount; i++)
                EndianBitConverter.Big.CopyBytes(longs[i], buffer, 0);
            sw.Stop();
            Console.Out.WriteLine("EndianBitConverter.Big.CopyBytes() took {0} ms", sw.ElapsedMilliseconds);

            sw = Stopwatch.StartNew();
            for(var i = 0; i < itersCount; i++)
                EndianBitConverter.Little.CopyBytes(longs[i], buffer, 0);
            sw.Stop();
            Console.Out.WriteLine("EndianBitConverter.Little.CopyBytes() took {0} ms", sw.ElapsedMilliseconds);
        }

        private const int iterationsOfRandom = 1000;
        private Random random;
    }
}