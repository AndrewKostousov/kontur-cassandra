using System.Collections.Generic;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Objects
{
    public class LruCache<TKey, TValue> where TValue : class
    {
        public LruCache(int cacheSize, IEqualityComparer<TKey> keyComparer = null)
        {
            this.cacheSize = cacheSize;
            items = new Dictionary<TKey, LinkedListNode<CacheItem>>(keyComparer ?? EqualityComparer<TKey>.Default);
        }

        [CanBeNull]
        public TValue TryGet([NotNull] TKey key)
        {
            LinkedListNode<CacheItem> listItem;
            if(!items.TryGetValue(key, out listItem))
                return null;
            Promote(listItem);
            return listItem.Value.value;
        }

        public void Add([NotNull] TKey key, [NotNull] TValue value)
        {
            if(value == null)
                throw new InvalidProgramStateException("Non-null value is required");
            if(items.Count >= cacheSize)
            {
                var first = itemsList.First;
                if(first != null)
                    Remove(first);
            }
            LinkedListNode<CacheItem> listItem;
            if(items.TryGetValue(key, out listItem))
            {
                listItem.Value.value = value;
                Promote(listItem);
            }
            else
            {
                var newItem = new CacheItem {key = key, value = value};
                var newListItem = itemsList.AddLast(newItem);
                items[key] = newListItem;
            }
        }

        public void Remove([NotNull] TKey key)
        {
            LinkedListNode<CacheItem> listItem;
            if(items.TryGetValue(key, out listItem))
                Remove(listItem);
        }

        private void Remove([NotNull] LinkedListNode<CacheItem> listItem)
        {
            items.Remove(listItem.Value.key);
            itemsList.Remove(listItem);
        }

        private void Promote([NotNull] LinkedListNode<CacheItem> listItem)
        {
            itemsList.Remove(listItem);
            itemsList.AddLast(listItem);
        }

        private readonly int cacheSize;
        private readonly Dictionary<TKey, LinkedListNode<CacheItem>> items;
        private readonly LinkedList<CacheItem> itemsList = new LinkedList<CacheItem>();

        private class CacheItem
        {
            [NotNull]
            public TKey key;

            [NotNull]
            public TValue value;
        }
    }
}