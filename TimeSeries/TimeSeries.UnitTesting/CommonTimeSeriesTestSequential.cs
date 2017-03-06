using System;
using System.Linq;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;
using FluentAssertions;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    public abstract class CommonTimeSeriesTestSequential : TimeSeriesTestBase
    {
        private Timestamp t00;
        private Timestamp t01;
        private Timestamp t02;
        private Timestamp t03;
        private Timestamp t10;
        private Timestamp t11;
        private Timestamp t20;
        private Timestamp t21;
        private Timestamp t30;
        private Event e00;
        private Event e01;
        private Event e02;
        private Event e03;
        private Event e11;
        private Event e21;
        private Event e30;

        /*
         *     e00  e01  e02  e03          e11              e21      e30
         *     |-------------------|----------------|----------------|
         *     t00  t01  t02  t03  t10     t11      t20     t21      t30
         */

        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            
            t00 = Timestamp.Now.Floor(Event.PartitionDutation);

            t10 = t00 + Event.PartitionDutation;
            t20 = t00 + Event.PartitionDutation.Multiply(2);
            t30 = t00 + Event.PartitionDutation.Multiply(3);

            t01 = t00 + Event.PartitionDutation.Divide(4);
            t02 = t00 + Event.PartitionDutation.Divide(2);
            t03 = t00 + Event.PartitionDutation.Divide(4).Multiply(3);

            t11 = t10 + Event.PartitionDutation.Divide(2);

            t21 = t20 + Event.PartitionDutation.Divide(2);
            
            e00 = EventAt(t00);
            e01 = EventAt(t01);
            e02 = EventAt(t02);
            e03 = EventAt(t03);
            e11 = EventAt(t11);
            e21 = EventAt(t21);
            e30 = EventAt(t30);
        }

        private Event EventAt(Timestamp time)
        {
            var ev = new Event(TimeGuid.NewGuid(time), new EventProto());
            Series.WriteWithoutSync(ev);

            return ev;
        }

        [Test]
        public void Read_ByTimestamp_FromSingleSlice()
        {
            Series.ReadRange(t01, t03).Cast<EventProto>().ShouldBeExactly(e02, e03);
        }

        [Test]
        public void Read_ByTimeGuid_FromSingleSlice()
        {
            Series.ReadRange(e01.TimeGuid, e03.TimeGuid).Cast<EventProto>().ShouldBeExactly(e02, e03);
        }

        [Test]
        public void Read_ByTimestamp_FromManySlices()
        {
            Series.ReadRange(t01, t21).Cast<EventProto>().ShouldBeExactly(e02, e03, e11, e21);
        }

        [Test]
        public void Read_ByTimeGuid_FromManySlices()
        {
            Series.ReadRange(e01.TimeGuid, e21.TimeGuid).Cast<EventProto>().ShouldBeExactly(e02, e03, e11, e21);
        }

        [Test]
        public void Read_ByTimestamp_StartIsGreaterThanEnd()
        {
            Series.ReadRange(t10, t00).Cast<EventProto>().Should().BeEmpty();
        }

        [Test]
        public void Read_ByTimeGuid_StartIsGreaterThanEnd()
        {
            Series.ReadRange(e11.TimeGuid, e00.TimeGuid).Cast<EventProto>().Should().BeEmpty();
        }

        [Test]
        public void Read_ByTimestamp_StartEqualsToEnd()
        {
            Series.ReadRange(t01, t01).Cast<EventProto>().Should().BeEmpty();
        }

        [Test]
        public void Read_ByTimeGuid_StartEqualsToEnd()
        {
            Series.ReadRange(e01.TimeGuid, e01.TimeGuid).Cast<EventProto>().Should().BeEmpty();
        }

        [Test]
        public void Read_ByTimestamp_StartEqualsToEnd_AndLieOnSliceBorder()
        {
            Series.ReadRange(t30, t30).Cast<EventProto>().Should().BeEmpty();
        }

        [Test]
        public void Read_ByTimeGuid_StartEqualsToEnd_AndLieOnSliceBorder()
        {
            Series.ReadRange(e30.TimeGuid, e30.TimeGuid).Cast<EventProto>().Should().BeEmpty();
        }

        [Test]
        public void Read_ByTimestamp_LastElementOnSliceBorder()
        {
            Series.ReadRange(t21, t30).Cast<EventProto>().ShouldBeExactly(e30);
        }

        [Test]
        public void Read_ByTimeGuid_LastElementOnSliceBorder()
        {
            Series.ReadRange(e21.TimeGuid, e30.TimeGuid).Cast<EventProto>().ShouldBeExactly(e30);
        }

        [Test]
        public void Read_ByTimestamp_FirstElementOnSliceBorder()
        {
            Series.ReadRange(t00, t01).Cast<EventProto>().ShouldBeExactly(e01);
        }

        [Test]
        public void Read_ByTimeGuid_FirstElementOnSliceBorder()
        {
            Series.ReadRange(e00.TimeGuid, e01.TimeGuid).Cast<EventProto>().ShouldBeExactly(e01);
        }

        [Test]
        public void ReadCount_ByTimestamp_FromSingleSlice()
        {
            Series.ReadRange(t00, t03, 2).Cast<EventProto>().ShouldBeExactly(e01, e02);
        }

        [Test]
        public void ReadCount_ByTimeGuid_FromSingleSlice()
        {
            Series.ReadRange(e00.TimeGuid, e03.TimeGuid, 2).Cast<EventProto>().ShouldBeExactly(e01, e02);
        }

        [Test]
        public void ReadCount_ByTimestamp_FromManySlices()
        {
            Series.ReadRange(t01, t21, 3).Cast<EventProto>().ShouldBeExactly(e02, e03, e11);
        }

        [Test]
        public void ReadCount_ByTimeGuid_FromManySlices()
        {
            Series.ReadRange(e01.TimeGuid, e21.TimeGuid, 3).Cast<EventProto>().ShouldBeExactly(e02, e03, e11);
        }

        [Test]
        public void Read_ByTimestamp_StartIsNull()
        {
            Series.ReadRange(null, t03).Cast<EventProto>().ShouldBeExactly(e00, e01, e02, e03);
        }

        [Test]
        public void Read_ByTimeGuid_StartIsNull()
        {
            Series.ReadRange(null, e03.TimeGuid).Cast<EventProto>().ShouldBeExactly(e00, e01, e02, e03);
        }

        [Test]
        public void Read_ByTimestamp_EndIsNull()
        {
            Series.ReadRange(t11, null).Cast<EventProto>().ShouldBeExactly(e21, e30);
        }

        [Test]
        public void Read_ByTimeGuid_EndIsNull()
        {
            Series.ReadRange(e11.TimeGuid, null).Cast<EventProto>().ShouldBeExactly(e21, e30);
        }

        [Test]
        public void Read_ByTimestamp_ReadAll()
        {
            Series.ReadRange((Timestamp)null, null).Cast<EventProto>().ShouldBeExactly(e00, e01, e02, e03, e11, e21, e30);
        }

        [Test]
        public void Read_ByTimeGuid_ReadAll()
        {
            Series.ReadRange((TimeGuid)null, null).Cast<EventProto>().ShouldBeExactly(e00, e01, e02, e03, e11, e21, e30);
        }

        [Test]
        public void ReadCount_ByTimestamp_ReadAll()
        {
            Series.ReadRange((Timestamp)null, null, 3).Cast<EventProto>().ShouldBeExactly(e00, e01, e02);
        }

        [Test]
        public void ReadCount_ByTimeGuid_ReadAll()
        {
            Series.ReadRange((TimeGuid)null, null, 3).Cast<EventProto>().ShouldBeExactly(e00, e01, e02);
        }
    }
}