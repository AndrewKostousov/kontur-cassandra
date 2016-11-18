using System;

namespace SKBKontur.Catalogue.StatisticsAggregator
{
    public class EmptyStatisticsAggregator : IStatisticsAggregator
    {
        public void AddEvent(int value)
        {
        }

        public void AddEvent(DateTime now, int value)
        {
        }

        public ValueWithInterval<long> GetCount(DateTime now, TimeSpan fromSeconds)
        {
            return new ValueWithInterval<long>(0, TimeSpan.FromSeconds(1));
        }

        public ValueWithInterval<long> GetSum(DateTime now, TimeSpan fromSeconds)
        {
            return new ValueWithInterval<long>(0, TimeSpan.FromSeconds(1));
        }

        public ValueWithInterval<long> GetQuantile(DateTime now, TimeSpan fromSeconds, int quantile)
        {
            return new ValueWithInterval<long>(0, TimeSpan.FromSeconds(1));
        }
    }
}