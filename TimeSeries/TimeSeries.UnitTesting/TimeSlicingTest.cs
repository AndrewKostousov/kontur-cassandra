using NUnit.Framework;
using System;
using CassandraTimeSeries.Utils;
using Commons;
using FluentAssertions;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class TimeSlicingTest
    {
        private TimeSpan sliceDuration;
        private Timestamp firstSliceStart;
        private Timestamp secondSliceStart;
        private Timestamp t1;
        private Timestamp t0;
        private Timestamp t2;

        [SetUp]
        public void SetUp()
        {
            sliceDuration = TimeSpan.FromMinutes(1);

            t1 = Timestamp.Now;
            t0 = t1 - sliceDuration;
            t2 = t1 + sliceDuration;

            firstSliceStart = t1.Floor(sliceDuration);
            secondSliceStart = firstSliceStart + sliceDuration;
        }

        [Test]
        public void Generic()
        {
            TimeSlicer.Slice(t1, t1 + sliceDuration.Multiply(2), sliceDuration).Should()
                .Equal(firstSliceStart, secondSliceStart, firstSliceStart + sliceDuration.Multiply(2));
        }

        [Test]
        public void UpperBound_IsLess_Than_LowerBound()
        {
            TimeSlicer.Slice(t1, t0, sliceDuration).Should().BeEmpty();
        }

        [Test]
        public void UpperBound_IsEqual_To_LowerBound()
        {
            TimeSlicer.Slice(t1, t1, sliceDuration).Should().BeEmpty();
        }

        [Test]
        public void UpperBound_IsEqual_To_LowerBound_And_SliceBorder()
        {
            TimeSlicer.Slice(firstSliceStart, firstSliceStart, sliceDuration).Should().BeEmpty();
        }

        [Test]
        public void UpperBound_IsEqual_To_SliceBorder()
        {
            TimeSlicer.Slice(t1, secondSliceStart, sliceDuration).ShouldBeExactly(firstSliceStart, secondSliceStart);
        }

        [Test]
        public void LowerBound_IsEqual_To_SliceBorder()
        {
            TimeSlicer.Slice(firstSliceStart, t2, sliceDuration).ShouldBeExactly(firstSliceStart, secondSliceStart);
        }
    }
}
