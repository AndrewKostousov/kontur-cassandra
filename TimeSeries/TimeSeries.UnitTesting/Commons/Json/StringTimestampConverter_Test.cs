using System;
using System.Collections.Generic;
using Commons;
using Commons.Json;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting.Commons.Json
{
    [TestFixture]
    public class StringTimestampConverterTest
    {
        [Test]
        [TestCaseSource("EnumTimestamps")]
        public void Write_Read([CanBeNull] Timestamp ts)
        {
            var s = JsonConvert.SerializeObject(new Item {Timestamp = ts});
            Console.Out.WriteLine(s);
            var actualTs = JsonConvert.DeserializeObject<Item>(s).Timestamp;
            Console.Out.WriteLine(actualTs);
            Assert.That(actualTs, Is.EqualTo(ts));
        }

        private static IEnumerable<Timestamp> EnumTimestamps()
        {
            yield return null;
            yield return Timestamp.MinValue;
            yield return Timestamp.MaxValue;
            yield return new Timestamp(new DateTime(2013, 07, 15, 17, 04, 56, DateTimeKind.Utc));
        }

        [Test]
        public void InvalidToken()
        {
            const string s = "{\"Timestamp\": \"Mon Jul 15 2013 23:04:56 GMT+0600 (Ekaterinburg Standard Time)\"}";
            var e = Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Item>(s));
            Assert.That(e.Message, Does.Contain("Unexpected token when parsing timestamp"));
        }

        private class Item
        {
            [JsonConverter(typeof(StringTimestampConverter))]
            public Timestamp Timestamp { get; set; }
        }
    }
}