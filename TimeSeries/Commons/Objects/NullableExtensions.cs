using System;

namespace SKBKontur.Catalogue.Objects
{
    public static class NullableExtensions
    {
        public static TResult? GetPropertyOrDefault<T, TResult>(this T? value, Func<T, TResult> propertyAccessor) where T: struct where TResult: struct
        {
            return value.HasValue ? propertyAccessor(value.Value) : default(TResult?);
        }
    }
}