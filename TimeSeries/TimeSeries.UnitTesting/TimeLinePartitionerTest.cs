using NUnit.Framework;
using System;
using CassandraTimeSeries.Utils;
using Commons;
using FluentAssertions;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class TimeLinePartitionerTest
    {
        private TimeLinePartitioner partitioner;

        private TimeSpan partitionDuration;
        private Timestamp firstSliceStart;
        private Timestamp secondSliceStart;
        private Timestamp t1;
        private Timestamp t0;
        private Timestamp t2;

        [SetUp]
        public void SetUp()
        {
            partitionDuration = TimeSpan.FromMinutes(1);
            partitioner = new TimeLinePartitioner(partitionDuration);

            t1 = Timestamp.Now;
            t0 = t1 - partitionDuration;
            t2 = t1 + partitionDuration;

            firstSliceStart = t1.Floor(partitionDuration);
            secondSliceStart = firstSliceStart + partitionDuration;
        }

        [Test]
        public void Generic()
        {
            partitioner.SplitIntoPartitions(t1, t1 + partitionDuration.Multiply(2)).Should()
                .Equal(firstSliceStart, secondSliceStart, firstSliceStart + partitionDuration.Multiply(2));
        }

        [Test]
        public void UpperBound_IsLess_Than_LowerBound()
        {
            partitioner.SplitIntoPartitions(t1, t0).Should().BeEmpty();
        }

        [Test]
        public void UpperBound_IsEqual_To_LowerBound()
        {
            partitioner.SplitIntoPartitions(t1, t1).Should().BeEmpty();
        }

        [Test]
        public void UpperBound_IsEqual_To_LowerBound_And_SliceBorder()
        {
            partitioner.SplitIntoPartitions(firstSliceStart, firstSliceStart).Should().BeEmpty();
        }

        [Test]
        public void UpperBound_IsEqual_To_SliceBorder()
        {
            partitioner.SplitIntoPartitions(t1, secondSliceStart).ShouldBeExactly(firstSliceStart, secondSliceStart);
        }

        [Test]
        public void LowerBound_IsEqual_To_SliceBorder()
        {
            partitioner.SplitIntoPartitions(firstSliceStart, t2).ShouldBeExactly(firstSliceStart, secondSliceStart);
        }
    }
}
