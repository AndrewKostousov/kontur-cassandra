using System;

namespace SKBKontur.Catalogue.StatisticsAggregator
{
    public class ValueWithInterval<T>
    {
        public ValueWithInterval(T value, TimeSpan interval)
        {
            Value = value;
            Interval = interval;
        }

        public ValueWithInterval<TResult> MapValue<TResult>(Func<T, TResult> mapFunc)
        {
            return new ValueWithInterval<TResult>(mapFunc(Value), Interval);
        }

        public T Value { get; private set; }
        public TimeSpan Interval { get; private set; }
    }
}