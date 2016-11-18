using System;
using System.Collections.Generic;
using System.Linq;

namespace SKBKontur.Catalogue.Linq
{
    public static class ZipExtensions
    {
        public static IEnumerable<TResult> EquiZipBy<T1, T2, TKey, TResult>(this IEnumerable<T1> items1, IEnumerable<T2> items2, Func<T1, TKey> keySelector1, Func<T2, TKey> keySelector2, Func<T1, T2, TResult> resultSelector)
        {
            return ZipBy(items1, items2, keySelector1, keySelector2, resultSelector, false);
        }

        public static IEnumerable<TResult> ZipBy<T1, T2, TKey, TResult>(this IEnumerable<T1> items1, IEnumerable<T2> items2, Func<T1, TKey> keySelector1, Func<T2, TKey> keySelector2, Func<T1, T2, TResult> resultSelector)
        {
            return ZipBy(items1, items2, keySelector1, keySelector2, resultSelector, true);
        }

        private static IEnumerable<TResult> ZipBy<T1, T2, TKey, TResult>(IEnumerable<T1> items1, IEnumerable<T2> items2, Func<T1, TKey> keySelector1, Func<T2, TKey> keySelector2, Func<T1, T2, TResult> resultSelector, bool returnDefaultIfNotContains)
        {
            var dict1 = items1.GroupBy(keySelector1).ToDictionary(x => x.Key, x => x.Single());
            var dict2 = items2.GroupBy(keySelector2).ToDictionary(x => x.Key, x => x.Single());
            var allKeys = dict1.Keys.Concat(dict2.Keys).Distinct();
            return allKeys.Select(key =>
                {
                    var value1 = GetValueFromDictionary(dict1, key, returnDefaultIfNotContains);
                    var value2 = GetValueFromDictionary(dict2, key, returnDefaultIfNotContains);
                    return resultSelector(value1, value2);
                });
        }

        private static TValue GetValueFromDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, bool returnDefaultIfNotContains)
        {
            if(dictionary.ContainsKey(key)) return dictionary[key];
            if(returnDefaultIfNotContains) return default(TValue);
            throw new KeyNotFoundException(key.ToString());
        }
    }
}