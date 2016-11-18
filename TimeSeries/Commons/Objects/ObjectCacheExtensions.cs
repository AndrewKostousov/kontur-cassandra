using System;
using System.Runtime.Caching;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Objects
{
    public static class ObjectCacheExtensions
    {
        [NotNull]
        public static T GetValueUsingCache<T>([NotNull] this ObjectCache cache, [NotNull] string cacheKey, TimeSpan cacheItemTtl, [NotNull] Func<T> createValue, bool slidingExpiration = false)
        {
            var result = cache[cacheKey];
            if(result == null)
            {
                result = createValue();
                cache.SetValue(cacheKey, cacheItemTtl, result, slidingExpiration);
            }
            return (T)result;
        }
        
        [CanBeNull]
        public static T TryGetValueUsingCache<T>([NotNull] this ObjectCache cache, [NotNull] string cacheKey, TimeSpan cacheItemTtl, [NotNull] Func<T> tryCreateValue, bool slidingExpiration = false)
        {
            var result = cache[cacheKey];
            if(result == null)
            {
                result = tryCreateValue();
                if (result != null)
                    cache.SetValue(cacheKey, cacheItemTtl, result, slidingExpiration);
            }
            return (T)result;
        }

        public static void SetValue<T>([NotNull] this ObjectCache cache, [NotNull] string cacheKey, TimeSpan cacheItemTtl, [NotNull] T value, bool slidingExpiration = false)
        {
            if(slidingExpiration)
                cache.Set(cacheKey, value, new CacheItemPolicy {SlidingExpiration = cacheItemTtl});
            else
                cache.Set(cacheKey, value, DateTimeOffset.UtcNow.Add(cacheItemTtl));
        }
    }
}