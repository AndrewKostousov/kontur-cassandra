using NUnit.Framework;
using System;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class TimeRoundingTest
    {
        [TestCase("11:48:15 +00:00", 60, "11:00:00 +00:00")]
        [TestCase("11:48:15 +00:00", 30, "11:30:00 +00:00")]
        [TestCase("11:48:15 +00:00", 10, "11:40:00 +00:00")]
        [TestCase("11:48:15 +00:00", 5, "11:45:00 +00:00")]
        [TestCase("11:48:15 +00:00", 1, "11:48:00 +00:00")]
        public void Rounding_ShouldBeCorrect(string offset, int preciseMinutes, string expected)
        {
            var offsetDate = DateTimeOffset.Parse(offset);
            var precise = TimeSpan.FromMinutes(preciseMinutes);
            var expectedDate = DateTimeOffset.Parse(expected);
            Assert.AreEqual(expectedDate, offsetDate.RoundDown(precise));
        }
        
        [Test]
        public void Rounded_ShouldBeEarlier()
        {
            var offset = DateTimeOffset.Parse("05/01/2008 11:20:30 +00:00");
            var rounded = offset.RoundDown(TimeSpan.FromMinutes(1));
            Assert.IsTrue(rounded < offset);
        }

        [Test]
        public void Rounded_ShouldBeTheSame_IfAlreadyRounded()
        {
            var offset = DateTimeOffset.Parse("05/01/2008 11:20 +00:00");
            var rounded = offset.RoundDown(TimeSpan.FromMinutes(1));
            Assert.AreEqual(offset, rounded);
        }

        [Test]
        public void Rounded_ShouldBeNotLessThanPrecise()
        {
            var offset = DateTimeOffset.Parse("05/01/2008 11:20:15 +00:00");
            var precise = TimeSpan.FromMinutes(1);
            var rounded = offset.RoundDown(precise);
            Assert.IsTrue(offset - rounded < precise);
        }
    }
}
