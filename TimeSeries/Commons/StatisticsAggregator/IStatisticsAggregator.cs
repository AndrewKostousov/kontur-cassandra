using System;

namespace SKBKontur.Catalogue.StatisticsAggregator
{
    public interface IStatisticsAggregator
    {
        void AddEvent(int value);
        void AddEvent(DateTime now, int value);
        ValueWithInterval<long> GetCount(DateTime now, TimeSpan fromSeconds);
        ValueWithInterval<long> GetSum(DateTime now, TimeSpan fromSeconds);
        ValueWithInterval<long> GetQuantile(DateTime now, TimeSpan fromSeconds, int quantile);
    }
}