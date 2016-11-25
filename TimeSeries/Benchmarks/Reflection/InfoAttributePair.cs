using System;

namespace Benchmarks
{
    static class InfoAttributePair
    {
        public static InfoAttributePair<TInfo, TAttribute> Create<TInfo, TAttribute>(
            TInfo info, TAttribute attribute) where TAttribute : Attribute
        {
            return new InfoAttributePair<TInfo, TAttribute>(info, attribute);
        }
    }

    class InfoAttributePair<TInfo, TAttribute>
        where TAttribute : Attribute
    {
        public TInfo Info { get; }
        public TAttribute Attribute { get; }

        public InfoAttributePair(TInfo info, TAttribute attribute)
        {
            Info = info;
            Attribute = attribute;
        }
    }
}