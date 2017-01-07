using System;
using Commons;
using NUnit.Framework;

namespace EdiTimeline.Tests
{
    [TestFixture]
    public class BoxEventWithEventContentEquivalencyStep_Test
    {
        [Test]
        public void Test()
        {
            var dcId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var entityId = Guid.NewGuid();
            var eventTimestamp = Timestamp.Now;
            var e0 = new BoxEvent(new BoxIdentifier("b1", "p1"), dcId, eventId, eventTimestamp, new Lazy<BoxEventContent>(() => new DummyEventContent(entityId)));
            var e1 = new BoxEvent(new BoxIdentifier("b1", "p1"), dcId, eventId, eventTimestamp, new Lazy<BoxEventContent>(() => new DummyEventContent(entityId)));
            var e2 = new BoxEvent(new BoxIdentifier("b2", "p1"), dcId, eventId, eventTimestamp, new Lazy<BoxEventContent>(() => new DummyEventContent(entityId)));
            var e3 = new BoxEvent(new BoxIdentifier("b1", "p1"), dcId, eventId, eventTimestamp, new Lazy<BoxEventContent>(() => null));
            e0.ShouldBeEquivalentTo(e1);
            Assert.Throws<AssertionException>(() => e0.ShouldBeEquivalentTo(e2));
            Assert.Throws<AssertionException>(() => e0.ShouldBeEquivalentTo(e3));
        }
    }
}