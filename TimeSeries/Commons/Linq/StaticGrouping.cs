using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SKBKontur.Catalogue.Linq
{
    public class StaticGrouping<TKey, TItem> : IGrouping<TKey, TItem>
    {
        public StaticGrouping(TKey key, IEnumerable<TItem> items)
        {
            Key = key;
            this.items = items;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TKey Key { get; private set; }
        private readonly IEnumerable<TItem> items;
    }

    public static class StaticGrouping
    {
        public static IGrouping<TKey, TItem> Create<TKey, TItem>(TKey key, IEnumerable<TItem> items)
        {
            return new StaticGrouping<TKey, TItem>(key, items);
        }
    }
}