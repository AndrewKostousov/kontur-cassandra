using System;
using System.Collections.Generic;

namespace SKBKontur.Catalogue.Linq
{
    public static class ArrayExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this Array array)
        {
            for(var i = 0; i < array.Length; i++)
            {
                if(!(array.GetValue(i) is T))
                    throw new ArgumentException(string.Format("Элемент массива не является типом {0}", typeof(T)), "array");
                yield return (T)array.GetValue(i);
            }
        }
    }
}