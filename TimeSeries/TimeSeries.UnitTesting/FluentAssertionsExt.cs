using System.Collections.Generic;
using FluentAssertions;

namespace CassandraTimeSeries.UnitTesting
{
    static class FluentAssertionsExt
    {
        public static void ShouldBeExactly<T>(this IEnumerable<T> subject, params T[] expectation)
        {
            subject.ShouldBeEquivalentTo(expectation, options => options.WithStrictOrderingFor(x => x));
        }
    }
}
