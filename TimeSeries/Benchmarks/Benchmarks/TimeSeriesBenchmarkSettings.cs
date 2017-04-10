using System;
using CassandraTimeSeries.ReadWrite;

namespace Benchmarks.Benchmarks
{
    public class TimeSeriesBenchmarkSettings
    {
        public static TimeSeriesBenchmarkSettings Default() => new TimeSeriesBenchmarkSettings();

        public ReaderSettings ReaderSettings { get; private set; } = new ReaderSettings();
        public WriterSettings WriterSettings { get; private set; } = new WriterSettings();
        public TimeSpan BenchmarkDuration { get; private set; } = TimeSpan.FromSeconds(10);
        public TimeSpan WarmUpDuration { get; private set; } = TimeSpan.FromSeconds(1);
        public int PreloadedEventsCount { get; private set; } = 10000;

        private TimeSeriesBenchmarkSettings() { }

        private TimeSeriesBenchmarkSettings(TimeSeriesBenchmarkSettings other)
        {
            ReaderSettings = other.ReaderSettings;
            WriterSettings = other.WriterSettings;
            BenchmarkDuration = other.BenchmarkDuration;
            WarmUpDuration = other.WarmUpDuration;
            PreloadedEventsCount = other.PreloadedEventsCount;
        }

        public TimeSeriesBenchmarkSettings WithReaderSettings(ReaderSettings settings)
        {
            return new TimeSeriesBenchmarkSettings(this) {ReaderSettings = settings};
        }

        public TimeSeriesBenchmarkSettings WithWriterSettings(WriterSettings settings)
        {
            return new TimeSeriesBenchmarkSettings(this) { WriterSettings = settings };
        }

        public TimeSeriesBenchmarkSettings WithBenchmarkDuration(TimeSpan duration)
        {
            return new TimeSeriesBenchmarkSettings(this) { BenchmarkDuration = duration };
        }

        public TimeSeriesBenchmarkSettings WithWarmUpDuration(TimeSpan duration)
        {
            return new TimeSeriesBenchmarkSettings(this) { WarmUpDuration = duration };
        }

        public TimeSeriesBenchmarkSettings WithPreloadedEventsCount(int count)
        {
            return new TimeSeriesBenchmarkSettings(this) { PreloadedEventsCount = count };
        }
    }
}