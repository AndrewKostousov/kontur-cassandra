namespace SKBKontur.Catalogue.Ranges
{
    public sealed class IncomparableRange<T>
    {
        public IncomparableRange(T lowerBound, T upperBound)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        public T LowerBound { get; private set; }
        public T UpperBound { get; private set; }
    }
    
    public sealed class IncomparableRange
    {
        public static IncomparableRange<T> Of<T>(T lowerBound, T upperBound)
        {
            return new IncomparableRange<T>(lowerBound, upperBound);
        }
    }
}