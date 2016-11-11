using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries.Tests
{
    [TestClass]
    public class TimeSlicingTest
    {
        [TestMethod]
        public void Slicing_ShouldIncludeStart()
        {
            var start = DateTimeOffset.Parse("05/01/2008 11:27:15 +00:00");
            var end = DateTimeOffset.Parse("05/01/2008 11:30:15 +00:00");
            var precise = TimeSpan.FromMinutes(1);

            Assert.IsTrue(new TimeSlices(start, end, precise).Min() < start);
        }

        [TestMethod]
        public void Slicing_ShouldExcludeEnd()
        {
            var start = DateTimeOffset.Parse("05/01/2008 11:27:15 +00:00");
            var end = DateTimeOffset.Parse("05/01/2008 11:30:15 +00:00");
            var precise = TimeSpan.FromMinutes(1);

            Assert.IsTrue(new TimeSlices(start, end, precise).Max() < end);
        }

        [TestMethod]
        public void Slicing_ShouldReturnCorrectCount()
        {
            var start = DateTimeOffset.Parse("05/01/2008 11:27:15 +00:00");
            var end = DateTimeOffset.Parse("05/01/2008 11:30:15 +00:00");
            var precise = TimeSpan.FromMinutes(1);

            Assert.AreEqual(new TimeSlices(start, end, precise).Count(), 4);
        }

        [TestMethod]
        public void Slices_ShouldBeEquallyDistributed()
        {
            var start = DateTimeOffset.Parse("05/01/2008 11:27:15 +00:00");
            var end = DateTimeOffset.Parse("05/01/2008 11:30:15 +00:00");
            var precise = TimeSpan.FromMinutes(1);

            var slices = new TimeSlices(start, end, precise).ToArray();

            for (var i = 0; i < slices.Length - 1; ++i)
                Assert.AreEqual(slices[i+1] - slices[i], precise);
        }

        [TestMethod]
        public void Slices_ShouldBeRounded()
        {
            var start = DateTimeOffset.Parse("05/01/2008 11:27:15 +00:00");
            var end = DateTimeOffset.Parse("05/01/2008 11:30:15 +00:00");
            var precise = TimeSpan.FromMinutes(1);

            foreach (var slice in new TimeSlices(start, end, precise))
                Assert.AreEqual(slice.RoundDown(precise), slice);
        }

        [TestMethod]
        public void Slices_ShouldReturnOneSlice_IfStartAndEndAreEqual()
        {
            var start = DateTimeOffset.Parse("05/01/2008 11:27:15 +00:00");
            var end = start;
            var precise = TimeSpan.FromMinutes(1);

            Assert.AreEqual(1, new TimeSlices(start, end, precise).Count());
        }

        [TestMethod]
        public void Slices_ShouldReturnZeroSlices_IfStartIsGreaterThanEnd()
        {
            var start = DateTimeOffset.Parse("05/01/2008 11:27:15 +00:00");
            var end = start - TimeSpan.FromMinutes(10);
            var precise = TimeSpan.FromMinutes(1);

            Assert.AreEqual(0, new TimeSlices(start, end, precise).Count());
        }
    }
}
