using System.Collections;

namespace SKBKontur.Catalogue.Objects.Sorting
{
    internal class CompareNullComprareDecorator : IComparer
    {
        public CompareNullComprareDecorator(IComparer comparer)
        {
            this.comparer = comparer;
        }

        public int Compare(object x, object y)
        {
            if (IsNull(x) && IsNull(y))
                return 0;

            if (IsNull(x))
                return -1;

            if (IsNull(y))
                return 1;

            return comparer.Compare(x, y);
        }

        private static bool IsNull(object value)
        {
            var s = value as string;
            if(s != null)
                return string.IsNullOrEmpty(s);
            return value == null;
        }

        private readonly IComparer comparer;
    }
}