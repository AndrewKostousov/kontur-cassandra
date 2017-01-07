using System;
using System.Collections.Generic;
using Commons;
using Commons.Bits;
using Commons.Testing;
using FluentAssertions;
using FluentAssertions.Equivalency;

namespace EdiTimeline.Tests
{
    public static class BoxEventFluentAssertionsExtensions
    {
        public static void ShouldBeEquivalentTo(this BoxEvent actual, BoxEvent expected, string because = "", params object[] becauseArgs)
        {
            actual.ShouldBeEquivalentTo(expected, config, because, becauseArgs);
        }

        public static void ShouldBeEquivalentWithOrderTo(this IEnumerable<BoxEvent> actual, params BoxEvent[] expected)
        {
            actual.ShouldBeEquivalentWithOrderTo((IEnumerable<BoxEvent>)expected);
        }

        public static void ShouldBeEquivalentWithOrderTo(this IEnumerable<BoxEvent> actual, IEnumerable<BoxEvent> expected, string because = "", params object[] becauseArgs)
        {
            actual.ShouldBeEquivalentWithOrderTo(expected, config, because, becauseArgs);
        }

        private static readonly Func<EquivalencyAssertionOptions<BoxEvent>, EquivalencyAssertionOptions<BoxEvent>> config = opt => opt.Using(new BoxEventWithEventContentEquivalencyStep());

        private class BoxEventWithEventContentEquivalencyStep : IEquivalencyStep
        {
            public bool CanHandle(IEquivalencyValidationContext context, IEquivalencyAssertionOptions config)
            {
                return config.GetSubjectType(context) == typeof(BoxEvent);
            }

            public bool Handle(IEquivalencyValidationContext context, IEquivalencyValidator parent, IEquivalencyAssertionOptions config)
            {
                var actualBoxEvent = (BoxEvent)context.Subject;
                var expectedBoxEvent = (BoxEvent)context.Expectation;
                actualBoxEvent.EventId.Should().Be(expectedBoxEvent.EventId);
                actualBoxEvent.EventTimestamp.Should().Be(expectedBoxEvent.EventTimestamp);
                ByteArrayComparer.Instance.Equals(actualBoxEvent.Payload, expectedBoxEvent.Payload)
                    .Should().BeTrue($"Expected payload: {expectedBoxEvent.Payload.ToHexString()}, Actual payload: {actualBoxEvent.Payload.ToHexString()} for EventId: {expectedBoxEvent.EventId}, EventTimestamp: {expectedBoxEvent.EventTimestamp}");
                return true;
            }
        }
    }
}