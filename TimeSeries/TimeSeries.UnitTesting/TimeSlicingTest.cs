using NUnit.Framework;
using System;
using System.Linq;
using CassandraTimeSeries.Utils;
using Commons;
using FluentAssertions;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class TimeSlicingTest
    {
        [SetUp]
        public void SetUp()
        {
            t1 = Timestamp.Now;
            sliceDuration = TimeSpan.FromMinutes(1);
            firstSliceStart = t1.Floor(sliceDuration);
            secondSliceStart = firstSliceStart + sliceDuration;
        }

        [Test]
        public void UpperBound_IsLess_Than_LowerBound()
        {
            var t1 = Timestamp.Now;
            var t2 = t1 - sliceDuration;
            TimeSlicer.Slice(t1, t2, sliceDuration).Should().BeEmpty();
        }

        [Test]
        public void UpperBound_IsEqual_To_LowerBound()
        {
            TimeSlicer.Slice(t1, t1, sliceDuration).Should().Equal(firstSliceStart);
        }

        [Test]
        public void Generic()
        {
            TimeSlicer.Slice(t1, t1+sliceDuration.Multiply(2), sliceDuration).Should().Equal(firstSliceStart, secondSliceStart, firstSliceStart+ sliceDuration.Multiply(2));
        }

        static readonly object[] testDataSource = 
        {
            new[] { "00:00:00 +00:00", "00:11:00 +00:00" },
            new[] { "00:00:00 +00:00", "00:11:11 +00:00" },
            new[] { "00:00:00 +00:00", "00:00:11 +00:00" },
            new[] { "00:00:11 +00:00", "00:00:11 +00:00" },
            new[] { "00:00:11 +00:00", "00:11:11 +00:00" },
        };

        private TimeSpan sliceDuration;
        private Timestamp firstSliceStart;
        private Timestamp secondSliceStart;
        private Timestamp t1;

        [TestCaseSource(nameof(testDataSource))]
        public void Slicing_ShouldIncludeStart(string startTime, string endTime)
        {
            var start = new Timestamp(DateTime.Parse(startTime));
            var end = new Timestamp(DateTimeOffset.Parse(endTime));
            var precise = TimeSpan.FromMinutes(1);

            TimeSlicer.Slice(start, end, precise).Min().Should().BeLessOrEqualTo(start);
        }

        [TestCaseSource(nameof(testDataSource))]
        public void Slicing_ShouldExcludeEnd_IfEndNotEqualToStart(string startTime, string endTime)
        {
            var start = new Timestamp(DateTime.Parse(startTime));
            var end = new Timestamp(DateTimeOffset.Parse(endTime));
            var precise = TimeSpan.FromMinutes(1);

            TimeSlicer.Slice(start, end, precise).Max().Should().BeLessThan(end);
        }

        [TestCaseSource(nameof(testDataSource))]
        public void Slicing_ShouldReturnCorrectCount(string startTime, string endTime)
        {
            var start = new Timestamp(DateTime.Parse(startTime));
            var end = new Timestamp(DateTimeOffset.Parse(endTime));
            var precise = TimeSpan.FromMinutes(1);

            var count = (end - start).Ticks / precise.Ticks + (end.Ticks % precise.Ticks == 0 ? 0 : 1);

            TimeSlicer.Slice(start, end, precise).LongCount().Should().Be(count);
        }

        [TestCaseSource(nameof(testDataSource))]
        public void Slicing_ShouldBeOrdered(string startTime, string endTime)
        {
            var start = new Timestamp(DateTime.Parse(startTime));
            var end = new Timestamp(DateTimeOffset.Parse(endTime));
            var precise = TimeSpan.FromMinutes(1);

            var slices = TimeSlicer.Slice(start, end, precise).ToArray();

            slices.OrderBy(x => x).Should().Equal(slices);
        }

        [TestCaseSource(nameof(testDataSource))]
        public void Slices_ShouldBeEquallyDistributed(string startTime, string endTime)
        {
            var start = new Timestamp(DateTime.Parse(startTime));
            var end = new Timestamp(DateTimeOffset.Parse(endTime));
            var precise = TimeSpan.FromMinutes(1);

            var slices = TimeSlicer.Slice(start, end, precise).ToArray();

            for (var i = 0; i < slices.Length - 1; ++i)
                (slices[i + 1] - slices[i]).Should().Be(precise);
        }

        [TestCaseSource(nameof(testDataSource))]
        public void Slices_ShouldBeRounded(string startTime, string endTime)
        {
            var start = new Timestamp(DateTime.Parse(startTime));
            var end = new Timestamp(DateTimeOffset.Parse(endTime));
            var precise = TimeSpan.FromMinutes(1);

            foreach (var slice in TimeSlicer.Slice(start, end, precise))
                slice.Floor(precise).Should().Be(slice);
        }

        [TestCaseSource(nameof(testDataSource))]
        public void Slices_ShouldReturnOneSlice_IfStartAndEndAreEqual(string startTime, string endTime)
        {
            var start = new Timestamp(DateTime.Parse(startTime));
            var end = start;
            var precise = TimeSpan.FromMinutes(1);

            TimeSlicer.Slice(start, end, precise).Count().Should().Be(1);
        }

        [TestCaseSource(nameof(testDataSource))]
        public void Slices_ShouldReturnZeroSlices_IfStartIsGreaterThanEnd(string startTime, string endTime)
        {
            var start = new Timestamp(DateTime.Parse(startTime));
            var end = start - TimeSpan.FromMinutes(10);
            var precise = TimeSpan.FromMinutes(1);

            TimeSlicer.Slice(start, end, precise).Count().Should().Be(0);
        }
    }
}
