﻿using System;
using System.Globalization;
using NUnit.Framework;

namespace Commons.Tests
{
    [TestFixture]
    public class TimestampExtensionsTest
    {
        [Test]
        public void TestFloor_Exception()
        {
            Assert.That(() => Timestamp.Now.Floor(TimeSpan.MinValue), Throws.InstanceOf<InvalidProgramStateException>().With.Message.Matches("Could not run Floor with -.* precision"));
            Assert.That(() => Timestamp.Now.Floor(TimeSpan.Zero), Throws.InstanceOf<InvalidProgramStateException>().With.Message.EqualTo("Could not run Floor with 00:00:00 precision"));
            Assert.That(() => Timestamp.Now.Floor(TimeSpan.FromSeconds(-1)), Throws.InstanceOf<InvalidProgramStateException>().With.Message.EqualTo("Could not run Floor with -00:00:01 precision"));
        }

        [Test]
        public void TestFloor_Ticks()
        {
            DoTestFloor("2016-08-26T11:15:59.8735027Z", TimeSpan.FromTicks(1), "2016-08-26T11:15:59.8735027Z");

            DoTestFloor("2016-08-26T11:15:59.8735020Z", TimeSpan.FromTicks(2), "2016-08-26T11:15:59.8735020Z");
            DoTestFloor("2016-08-26T11:15:59.8735021Z", TimeSpan.FromTicks(2), "2016-08-26T11:15:59.8735020Z");
            DoTestFloor("2016-08-26T11:15:59.8735026Z", TimeSpan.FromTicks(2), "2016-08-26T11:15:59.8735026Z");
            DoTestFloor("2016-08-26T11:15:59.8735027Z", TimeSpan.FromTicks(2), "2016-08-26T11:15:59.8735026Z");
            DoTestFloor("2016-08-26T11:15:59.8735029Z", TimeSpan.FromTicks(2), "2016-08-26T11:15:59.8735028Z");

            DoTestFloor("2016-08-26T11:15:59.8735020Z", TimeSpan.FromTicks(5), "2016-08-26T11:15:59.8735020Z");
            DoTestFloor("2016-08-26T11:15:59.8735023Z", TimeSpan.FromTicks(5), "2016-08-26T11:15:59.8735020Z");
            DoTestFloor("2016-08-26T11:15:59.8735025Z", TimeSpan.FromTicks(5), "2016-08-26T11:15:59.8735025Z");
            DoTestFloor("2016-08-26T11:15:59.8735027Z", TimeSpan.FromTicks(5), "2016-08-26T11:15:59.8735025Z");
        }

        [Test]
        public void TestFloor_Milliseconds()
        {
            DoTestFloor("2016-08-26T11:15:59.8710000Z", TimeSpan.FromMilliseconds(3), "2016-08-26T11:15:59.8710000Z");
            DoTestFloor("2016-08-26T11:15:59.8735020Z", TimeSpan.FromMilliseconds(3), "2016-08-26T11:15:59.8710000Z");
            DoTestFloor("2016-08-26T11:15:59.8725020Z", TimeSpan.FromMilliseconds(3), "2016-08-26T11:15:59.8710000Z");
            DoTestFloor("2016-08-26T11:15:59.8695020Z", TimeSpan.FromMilliseconds(3), "2016-08-26T11:15:59.8680000Z");
        }
        
        [Test]
        public void TestFloor_Seconds()
        {
            DoTestFloor("2016-08-26T11:15:59.8710000Z", TimeSpan.FromSeconds(1), "2016-08-26T11:15:59.0000000Z");

            DoTestFloor("2016-08-26T11:15:01.8735020Z", TimeSpan.FromSeconds(13), "2016-08-26T11:14:59.0000000Z");
            DoTestFloor("2016-08-26T11:15:25.8735020Z", TimeSpan.FromSeconds(13), "2016-08-26T11:15:25.0000000Z");
            DoTestFloor("2016-08-26T11:15:26.8735020Z", TimeSpan.FromSeconds(13), "2016-08-26T11:15:25.0000000Z");
            DoTestFloor("2016-08-26T11:15:59.8735020Z", TimeSpan.FromSeconds(13), "2016-08-26T11:15:51.0000000Z");
        }
        
        [Test]
        public void TestFloor_Minutes()
        {
            DoTestFloor("2016-08-26T11:15:59.8710000Z", TimeSpan.FromMinutes(1), "2016-08-26T11:15:00.0000000Z");

            DoTestFloor("2016-08-26T11:05:59.8710000Z", TimeSpan.FromMinutes(10), "2016-08-26T11:00:00.0000000Z");
            DoTestFloor("2016-08-26T11:15:59.8710000Z", TimeSpan.FromMinutes(10), "2016-08-26T11:10:00.0000000Z");
        }
        
        [Test]
        public void TestFloor_Hours()
        {
            DoTestFloor("2016-08-26T11:15:59.8710000Z", TimeSpan.FromHours(1), "2016-08-26T11:00:00.0000000Z");

            DoTestFloor("2016-08-26T01:00:00.0000000Z", TimeSpan.FromHours(5), "2016-08-26T01:00:00.0000000Z");
            DoTestFloor("2016-08-26T03:00:00.0000000Z", TimeSpan.FromHours(5), "2016-08-26T01:00:00.0000000Z");
            DoTestFloor("2016-08-26T11:05:59.8710000Z", TimeSpan.FromHours(5), "2016-08-26T11:00:00.0000000Z");
            DoTestFloor("2016-08-26T17:05:59.8710000Z", TimeSpan.FromHours(5), "2016-08-26T16:00:00.0000000Z");
        }

        [Test]
        public void TestFloor_Days()
        {
            DoTestFloor("2016-08-26T11:15:59.8710000Z", TimeSpan.FromDays(1), "2016-08-26T00:00:00.0000000Z");

            DoTestFloor("2016-08-25T11:15:59.8710000Z", TimeSpan.FromDays(2), "2016-08-25T00:00:00.0000000Z");
            DoTestFloor("2016-08-28T11:15:59.8710000Z", TimeSpan.FromDays(2), "2016-08-27T00:00:00.0000000Z");
        }

        private static void DoTestFloor(string input, TimeSpan precision, string expected)
        {
            Assert.That(Parse(input).Floor(precision), Is.EqualTo(Parse(expected)));
        }

        private static Timestamp Parse(string str)
        {
            DateTime result;
            if (!DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AssumeUniversal, out result))
                throw new InvalidProgramStateException(string.Format("String {0} has not been recognized as correct DateTime representation", str));
            return new Timestamp(result);
        }
    }
}