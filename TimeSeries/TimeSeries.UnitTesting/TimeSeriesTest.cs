using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;
using FluentAssertions;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class TimeSeriesTest : TimeSeriesTestBase
    {
        protected override ITimeSeries TimeSeriesFactory(DatabaseWrapper wrapper)
        {
            return new TimeSeries(wrapper.Table);
        }
    }

    [TestFixture]
    public abstract class TimeSeriesTestBase
    {
        protected DatabaseWrapper wrapper;
        protected ITimeSeries series;

        private Timestamp t00;
        private Timestamp t01;
        private Timestamp t02;
        private Timestamp t10;
        private Timestamp t20;
        private Timestamp t21;
        private Timestamp t30;
        private Event e00;
        private Event e01;
        private Event e02;
        private Event e11;
        private Event e21;
        private Event e30;
        private Timestamp t03;
        private Event e03;
        private Timestamp t11;

        /*
         *     e00  e01  e02  e03          e11              e21      e30
         *     |-------------------|----------------|----------------|
         *     t00  t01  t02  t03  t10     t11      t20     t21      t30
         */

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            wrapper = new DatabaseWrapper("test");
            wrapper.Table.Drop();
            wrapper.Table.Create();
            series = TimeSeriesFactory(wrapper);

            t00 = Timestamp.Now.Floor(Event.SliceDutation);

            t10 = t00 + Event.SliceDutation;
            t20 = t00 + Event.SliceDutation.Multiply(2);
            t30 = t00 + Event.SliceDutation.Multiply(3);

            t01 = t00 + Event.SliceDutation.Divide(4);
            t02 = t00 + Event.SliceDutation.Divide(2);
            t03 = t00 + Event.SliceDutation.Divide(4).Multiply(3);

            t11 = t10 + Event.SliceDutation.Divide(2);

            t21 = t20 + Event.SliceDutation.Divide(2);
            
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
            wrapper.Table.Insert(ev).Execute();

            return ev;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            wrapper.Dispose();
        }
        
        protected abstract ITimeSeries TimeSeriesFactory(DatabaseWrapper wrapper);

        [Test]
        public void Read_ByTimestamp_FromSingleSlice()
        {
            series.ReadRange(t01, t03).ShouldBeExactly(e02, e03);
        }

        [Test]
        public void Read_ByTimeGuid_FromSingleSlice()
        {
            series.ReadRange(e01.TimeGuid, e03.TimeGuid).ShouldBeExactly(e02, e03);
        }

        [Test]
        public void Read_ByTimestamp_FromManySlices()
        {
            series.ReadRange(t01, t21).ShouldBeExactly(e02, e03, e11, e21);
        }

        [Test]
        public void Read_ByTimeGuid_FromManySlices()
        {
            series.ReadRange(e01.TimeGuid, e21.TimeGuid).ShouldBeExactly(e02, e03, e11, e21);
        }

        [Test]
        public void Read_ByTimestamp_StartIsGreaterThanEnd()
        {
            series.ReadRange(t10, t00).Should().BeEmpty();
        }

        [Test]
        public void Read_ByTimeGuid_StartIsGreaterThanEnd()
        {
            series.ReadRange(e11.TimeGuid, e00.TimeGuid).Should().BeEmpty();
        }

        [Test]
        public void Read_ByTimestamp_StartEqualsToEnd()
        {
            series.ReadRange(t01, t01).Should().BeEmpty();
        }

        [Test]
        public void Read_ByTimeGuid_StartEqualsToEnd()
        {
            series.ReadRange(e01.TimeGuid, e01.TimeGuid).Should().BeEmpty();
        }

        [Test]
        public void Read_ByTimestamp_LastElementOnSliceBorder()
        {
            series.ReadRange(t21, t30).ShouldBeExactly(e30);
        }

        [Test]
        public void Read_ByTimeGuid_LastElementOnSliceBorder()
        {
            series.ReadRange(e21.TimeGuid, e30.TimeGuid).ShouldBeExactly(e30);
        }

        [Test]
        public void Read_ByTimestamp_FirstElementOnSliceBorder()
        {
            series.ReadRange(t00, t01).ShouldBeExactly(e01);
        }

        [Test]
        public void Read_ByTimeGuid_FirstElementOnSliceBorder()
        {
            series.ReadRange(e00.TimeGuid, e01.TimeGuid).ShouldBeExactly(e01);
        }

        [Test]
        public void ReadCount_ByTimestamp_FromSingleSlice()
        {
            series.ReadRange(t00, t03, 2).ShouldBeExactly(e01, e02);
        }

        [Test]
        public void ReadCount_ByTimeGuid_FromSingleSlice()
        {
            series.ReadRange(e00.TimeGuid, e03.TimeGuid, 2).ShouldBeExactly(e01, e02);
        }

        [Test]
        public void ReadCount_ByTimestamp_FromManySlices()
        {
            series.ReadRange(t01, t21, 3).ShouldBeExactly(e02, e03, e11);
        }

        [Test]
        public void ReadCount_ByTimeGuid_FromManySlices()
        {
            series.ReadRange(e01.TimeGuid, e21.TimeGuid, 3).ShouldBeExactly(e02, e03, e11);
        }

        [Test]
        public void Read_ByTimestamp_StartIsNull()
        {
            series.ReadRange(null, t03).ShouldBeExactly(e00, e01, e02, e03);
        }

        [Test]
        public void Read_ByTimeGuid_StartIsNull()
        {
            series.ReadRange(null, e03.TimeGuid).ShouldBeExactly(e00, e01, e02, e03);
        }

        [Test]
        public void Read_ByTimestamp_EndIsNull()
        {
            series.ReadRange(t11, null).ShouldBeExactly(e21, e30);
        }

        [Test]
        public void Read_ByTimeGuid_EndIsNull()
        {
            series.ReadRange(e11.TimeGuid, null).ShouldBeExactly(e21, e30);
        }

        [Test]
        public void Read_ByTimestamp_ReadAll()
        {
            series.ReadRange((Timestamp)null, null).ShouldBeExactly(e00, e01, e02, e03, e11, e21, e30);
        }

        [Test]
        public void Read_ByTimeGuid_ReadAll()
        {
            series.ReadRange((TimeGuid)null, null).ShouldBeExactly(e00, e01, e02, e03, e11, e21, e30);
        }

        [Test]
        public void ReadCount_ByTimestamp_ReadAll()
        {
            series.ReadRange((Timestamp)null, null, 3).ShouldBeExactly(e00, e01, e02);
        }

        [Test]
        public void ReadCount_ByTimeGuid_ReadAll()
        {
            series.ReadRange((TimeGuid)null, null, 3).ShouldBeExactly(e00, e01, e02);
        }
    }
}
