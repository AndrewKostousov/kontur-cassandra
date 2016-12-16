using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Collections;

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
