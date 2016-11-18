using System;
using System.Threading;

namespace SKBKontur.Catalogue.StatisticsAggregator
{
    internal class StatisticsCell
    {
        public void AddValue(int value)
        {
            Interlocked.Add(ref sum, value);
            Interlocked.Increment(ref count);

            var index = Math.Min((int)Math.Round(Math.Log10(Math.Max(value, 1)) * 30), counts.Length - 1);
            Interlocked.Increment(ref counts[index]);

            var localMax = Interlocked.Read(ref max);
            while(value > localMax)
            {
                Interlocked.CompareExchange(ref max, value, localMax);
                localMax = Interlocked.Read(ref max);
            }
        }

        public long GetQuantile(int quantile)
        {
            var index = (int)Math.Round(Count * ((double)quantile / 100));
            var quantileBoundCount = 0;
            for(var i = 0; i < counts.Length; i++)
            {
                quantileBoundCount += counts[i];
                if(quantileBoundCount >= index)
                    return (int)Math.Round(Math.Pow(10, (double)i / 30));
            }
            return Max;
        }

        public long Count { get { return Interlocked.Read(ref count); } }
        public long Sum { get { return Interlocked.Read(ref sum); } }
        public long Max { get { return Interlocked.Read(ref max); } }
        internal readonly int[] counts = new int[250];

        private long max;
        private long count;
        private long sum;
    }
}