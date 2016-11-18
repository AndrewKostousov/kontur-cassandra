using System;
using System.Collections.Generic;
using System.Linq;

namespace SKBKontur.Catalogue.Linq
{
    public static class DistinctExtensions
    {
        public static T[] SortedDistinct<T>(this IEnumerable<T> enumerable, Comparison<T> comparison)
        {
            var arr = enumerable.ToArray();
            Array.Sort(arr, comparison);
            return arr.Where((el, idx) => idx == 0 || comparison(arr[idx - 1], el) != 0).ToArray();
        }
    }
}