using System;

namespace SKBKontur.Catalogue.Linq
{
    public static class Lazy
    {
        public static Lazy<T> Create<T>(Func<T> valueFactory)
        {
            return new Lazy<T>(valueFactory);
        }
    }
}