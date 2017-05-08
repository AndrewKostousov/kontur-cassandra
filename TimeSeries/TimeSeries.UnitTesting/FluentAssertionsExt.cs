using System.Collections.Generic;
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
    }
}
