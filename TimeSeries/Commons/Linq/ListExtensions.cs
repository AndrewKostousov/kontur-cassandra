using System.Collections.Generic;

namespace SKBKontur.Catalogue.Linq
{
    public static class ListExtensions
    {
         public static void RemoveTailFrom<T>(this List<T> list, int startIndex)
         {
             list.RemoveRange(startIndex, list.Count - startIndex);
         }
    }
}