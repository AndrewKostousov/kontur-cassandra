using System;
using System.Collections.Generic;
using System.Linq;

namespace SKBKontur.Catalogue.Linq
{
    public static class GroupingExtensions
    {
        public static IEnumerable<IGrouping<TResultKey, TResultItem>> Transform<TKey, TItem, TResultKey, TResultItem>(this IEnumerable<IGrouping<TKey, TItem>> groupings, Func<TKey, TResultKey> keyTransformer, Func<TItem, TResultItem> itemTransformer)
        {
            return groupings.Select(grouping => StaticGrouping.Create(keyTransformer(grouping.Key), grouping.Select(itemTransformer)));
        }

        public static IEnumerable<IGrouping<TKey, TResultValue>> ToGroupingEnumerable<TKey, TValue, TResultValue>(this Dictionary<TKey, TValue[]> dictionary, Func<TValue, TResultValue> valueSelector)
        {
            return dictionary.Select(keyValuePair => new StaticGrouping<TKey, TResultValue>(keyValuePair.Key, keyValuePair.Value.Select(valueSelector)));
        }
    }
}