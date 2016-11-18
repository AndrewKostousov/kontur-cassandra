using System;
using System.Collections;
using System.Collections.Generic;

using SKBKontur.Catalogue.Objects.ValueExtracting;

namespace SKBKontur.Catalogue.Objects.Sorting
{
    public class BySortRulesComparer<T> : IComparer<T> where T : class
    {
        public BySortRulesComparer(SortRule[] rules)
        {
            this.rules = rules;
        }

        public int Compare(T x, T y)
        {
            foreach(var rule in rules)
            {
                var value1 = ObjectValueExtractor.Extract(x, rule.Column);
                var value2 = ObjectValueExtractor.Extract(y, rule.Column);
                var comparer = new CompareNullComprareDecorator(GetComparer(typeof(T), rule.Column));
                var cmpRes = comparer.Compare(value1, value2) * (rule.SortMode == SortMode.Ascending ? 1 : -1);
                if(cmpRes != 0)
                    return cmpRes;
            }
            return 0;
        }

        private static IComparer GetComparer(Type rootType, string pathToProperty)
        {
            if (ObjectValueExtractor.GetPropertyType(rootType, pathToProperty) == typeof(string))
                return StringComparer.OrdinalIgnoreCase;
            return Comparer.Default;
        }

        private readonly SortRule[] rules;
    }
}