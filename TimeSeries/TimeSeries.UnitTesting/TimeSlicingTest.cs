using NUnit.Framework;
using System;
using System.Linq;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class TimeSlicingTest
    {
        static object[] TestDataSource = 
        {
            new string[] { "00:00:00 +00:00", "00:11:00 +00:00" },
            new string[] { "00:00:00 +00:00", "00:11:11 +00:00" },
            new string[] { "00:00:00 +00:00", "00:00:11 +00:00" },
            new string[] { "00:00:11 +00:00", "00:00:11 +00:00" },
            new string[] { "00:00:11 +00:00", "00:11:11 +00:00" },
        };

        [TestCaseSource("TestDataSource")]
        public void Slicing_ShouldIncludeStart(string startTime, string endTime)
        {
            var start = DateTimeOffset.Parse(startTime);
            var end = DateTimeOffset.Parse(endTime);
            var precise = TimeSpan.FromMinutes(1);

            Assert.LessOrEqual(new TimeSlices(start, end, precise).Min(), start);
        }

        [TestCaseSource("TestDataSource")]
        public void Slicing_ShouldExcludeEnd_IfEndNotEqualToStart(string startTime, string endTime)
        {
            var start = DateTimeOffset.Parse(startTime);
            var end = DateTimeOffset.Parse(endTime);
            var precise = TimeSpan.FromMinutes(1);

            var lastSlice = new TimeSlices(start, end, precise).Max();
            
            Assert.Less(lastSlice, end);
        }

        [TestCaseSource("TestDataSource")]
        public void Slicing_ShouldReturnCorrectCount(string startTime, string endTime)
        {
            var start = DateTimeOffset.Parse(startTime);
            var end = DateTimeOffset.Parse(endTime);
            var precise = TimeSpan.FromMinutes(1);

            var count = (end - start).Ticks / precise.Ticks + (end.Ticks % precise.Ticks == 0 ? 0 : 1); 

            Assert.AreEqual(count, new TimeSlices(start, end, precise).Count());
        }

        [TestCaseSource("TestDataSource")]
        public void Slicing_ShouldBeOrdered(string startTime, string endTime)
        {
            var start = DateTimeOffset.Parse(startTime);
            var end = DateTimeOffset.Parse(endTime);
            var precise = TimeSpan.FromMinutes(1);

            var slices = new TimeSlices(start, end, precise).ToArray();

            Assert.AreEqual(slices.OrderBy(x => x), slices);
        }

        [TestCaseSource("TestDataSource")]
        public void Slices_ShouldBeEquallyDistributed(string startTime, string endTime)
        {
            var start = DateTimeOffset.Parse(startTime);
            var end = DateTimeOffset.Parse(endTime);
            var precise = TimeSpan.FromMinutes(1);

            var slices = new TimeSlices(start, end, precise).ToArray();

            for (var i = 0; i < slices.Length - 1; ++i)
                Assert.AreEqual(precise, slices[i + 1] - slices[i]);
        }

        [TestCaseSource("TestDataSource")]
        public void Slices_ShouldBeRounded(string startTime, string endTime)
        {
            var start = DateTimeOffset.Parse(startTime);
            var end = DateTimeOffset.Parse(endTime);
            var precise = TimeSpan.FromMinutes(1);

            foreach (var slice in new TimeSlices(start, end, precise))
                Assert.AreEqual(slice, slice.RoundDown(precise));
        }

        [TestCaseSource("TestDataSource")]
        public void Slices_ShouldReturnOneSlice_IfStartAndEndAreEqual(string startTime, string endTime)
        {
            var start = DateTimeOffset.Parse(startTime);
            var end = start;
            var precise = TimeSpan.FromMinutes(1);

            Assert.AreEqual(1, new TimeSlices(start, end, precise).Count());
        }

        [TestCaseSource("TestDataSource")]
        public void Slices_ShouldReturnZeroSlices_IfStartIsGreaterThanEnd(string startTime, string endTime)
        {
            var start = DateTimeOffset.Parse(startTime);
            var end = start - TimeSpan.FromMinutes(10);
            var precise = TimeSpan.FromMinutes(1);

            Assert.AreEqual(0, new TimeSlices(start, end, precise).Count());
        }
    }
}
