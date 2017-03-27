using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TRes> Product<TLeft, TRight, TRes>(this IEnumerable<TLeft> left,
            IEnumerable<TRight> right, Func<TLeft, TRight, TRes> resultSelector)
        {
            var rights = right.ToArray();

            return left.SelectMany(x => rights.Select(y => resultSelector(x, y)));
        }
    }
}
