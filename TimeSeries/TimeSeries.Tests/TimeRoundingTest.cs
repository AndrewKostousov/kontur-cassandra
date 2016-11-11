using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CassandraTimeSeries.Tests
{
    [TestClass]
    public class TimeRoundingTest
    {
        [TestMethod]
        public void Rounding_ShouldBeCorrect_ForDifferentPrecises()
        {
            var offset = DateTimeOffset.Parse("05/01/2008 11:27:15 +00:00");
            var precise = TimeSpan.FromMinutes(10);
            var expected = DateTimeOffset.Parse("05/01/2008 11:20:00 +00:00");
            Assert.AreEqual(expected, offset.RoundDown(precise));

            offset = DateTimeOffset.Parse("05/01/2008 11:27:15 +00:00");
            precise = TimeSpan.FromMinutes(5);
            expected = DateTimeOffset.Parse("05/01/2008 11:25:00 +00:00");
            Assert.AreEqual(expected, offset.RoundDown(precise));
        }

        [TestMethod]
        public void Rounded_ShouldBeEarlier()
        {
            var offset = DateTimeOffset.Parse("05/01/2008 11:20:30 +00:00");
            var rounded = offset.RoundDown(TimeSpan.FromMinutes(1));
            Assert.IsTrue(rounded < offset);
        }

        [TestMethod]
        public void Rounded_ShouldBeSame_IfAlreadyRounded()
        {
            var offset = DateTimeOffset.Parse("05/01/2008 11:20 +00:00");
            var rounded = offset.RoundDown(TimeSpan.FromMinutes(1));
            Assert.AreEqual(offset, rounded);
        }

        [TestMethod]
        public void Rounded_ShouldBeNotLessThanPrecise()
        {
            var offset = DateTimeOffset.Parse("05/01/2008 11:20:15 +00:00");
            var precise = TimeSpan.FromMinutes(1);
            var rounded = offset.RoundDown(precise);
            Assert.IsTrue(offset - rounded < precise);
        }
    }
}
