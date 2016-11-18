using System;
using System.Collections.Generic;
using System.Linq;

namespace SKBKontur.Catalogue.Linq
{
    public static class RandomExtensions
    {
        public static T RandomElement<T>(this IEnumerable<T> enumerable, Random random)
        {
            if(enumerable == null)
                throw new ArgumentNullException("enumerable");
            var listAsList = enumerable as IList<T> ?? enumerable.ToList();
            var count = listAsList.Count();
            if(count == 0)
                throw new ArgumentException("List should contains at least one element", "enumerable");
            return listAsList.ElementAt(random.Next(count));
        }

        public static IEnumerable<T> RandomElements<T>(this IEnumerable<T> enumerable, Random random, int count)
        {
            if(enumerable == null)
                throw new ArgumentNullException("enumerable");
            var listAsList = enumerable as IList<T> ?? enumerable.ToList();
            var totalCount = listAsList.Count();
            if(totalCount == 0)
                throw new ArgumentException("List should contains at least one element", "enumerable");
            if(totalCount < count)
                throw new ArgumentException("List should contains more elements than count", "enumerable");

            var usedIndexes = new HashSet<int>();
            for(var i = 0; i < count; i++)
            {
                var index = random.Next(totalCount);
                while(usedIndexes.Contains(index))
                    index = random.Next(totalCount);
                usedIndexes.Add(index);
                yield return listAsList.ElementAt(index);
            }
        }
    }
}