using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Linq
{
    public static class CatalogueEnumerable
    {
        public static IEnumerable<long> Range(long start, long count)
        {
            if(count < 0)
                throw new ArgumentOutOfRangeException("count");
            var max = (ulong)start + (ulong)count - 1;
            if(max > long.MaxValue)
                throw new ArgumentOutOfRangeException("count");
            return RangeIterator(start, count);
        }

        public static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> lookup, TKey key)
        {
            TValue result;
            return lookup.TryGetValue(key, out result) ? result : default(TValue);
        }

        public static IEnumerable<TValue> ValueOrEmpty<TKey, TValue>(this ILookup<TKey, TValue> lookup, TKey key)
        {
            return lookup.Contains(key) ? lookup[key] : new TValue[0];
        }

        public static TSource Max<TSource>(this IEnumerable<TSource> sequence, Func<TSource, TSource, int> compareFunc)
        {
            var hasResult = false;
            var res = default(TSource);
            foreach(var source in sequence)
            {
                if(!hasResult || compareFunc(res, source) < 0)
                {
                    res = source;
                    hasResult = true;
                }
            }

            if(!hasResult)
                throw new ArgumentException("Sequence is empty", "sequence");
            return res;
        }

        [NotNull]
        public static IEnumerable<List<T>> BySizeBatch<T>([NotNull] this IEnumerable<T> sequence, [NotNull] Func<T, long> sizeFunc, long size)
        {
            var currentBatch = new List<T>();
            long currentSize = 0;
            foreach(var item in sequence)
            {
                var rowSize = sizeFunc(item);
                if(currentSize + rowSize > size && currentBatch.Count > 0)
                {
                    var nextRowToReturn = currentBatch;
                    currentBatch = new List<T>();
                    currentSize = 0;
                    yield return nextRowToReturn;
                }
                currentBatch.Add(item);
                currentSize += rowSize;
            }
            yield return currentBatch;
        }

        private static IEnumerable<long> RangeIterator(long start, long count)
        {
            for(var i = 0; i < count; i++) yield return start + i;
        }
    }
}