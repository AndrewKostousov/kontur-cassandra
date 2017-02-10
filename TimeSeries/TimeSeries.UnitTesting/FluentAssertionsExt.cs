using System.Collections.Generic;
using CassandraTimeSeries.Model;
using FluentAssertions;

namespace CassandraTimeSeries.UnitTesting
{
    static class FluentAssertionsExt
    {
        public static void ShouldBeExactly<T>(this IEnumerable<T> subject, params T[] expectation)
        {
            subject.ShouldBeExactly((IEnumerable<T>)expectation);
        }

        public static void ShouldBeExactly<T>(this IEnumerable<T> subject, IEnumerable<T> expectation)
        {
            subject.ShouldAllBeEquivalentTo(expectation, options => options.WithStrictOrderingFor(x => x));
        }

        public static void ShouldBeExactly(this IEnumerable<Event> subject, params Event[] expectation)
        {
            subject.ShouldBeExactly((IEnumerable<Event>)expectation);
        }

        public static void ShouldBeExactly(this IEnumerable<Event> subject, IEnumerable<Event> expectation)
        {
            subject.ShouldAllBeEquivalentTo(expectation, options => options.WithStrictOrderingFor(x => x).Excluding(e => e.MaxId));
        }
    }
}
