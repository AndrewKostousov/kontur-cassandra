using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Benchmarks.ReadWrite;
using Benchmarks.Reflection;
using Benchmarks.Results;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using CassandraTimeSeries.Utils;

namespace Benchmarks.Benchmarks
{
    [BenchmarkClass]
    public abstract class BaseTimeSeriesBenchmark
    {
        protected abstract IDatabaseController Database { get; }

        protected ITimeSeries Series { get; private set; }
        private ReadersWritersPool pool;

        private List<BenchmarkEventReader> readers;
        private List<BenchmarkEventWriter> writers;

        private readonly ReaderSettings readerSettings;
        private readonly WriterSettings writerSettings;

        protected abstract int ReadersCount { get; }
        protected abstract int WritersCount { get; }

        protected int CurrentIteration { get; private set; }

        protected BaseTimeSeriesBenchmark()
        {
            readerSettings = new ReaderSettings();
            writerSettings = new WriterSettings();
        }

        protected abstract ITimeSeries TimeSeriesFactory();

        [BenchmarkSetUp]
        public virtual void SetUp()
        {
            readers = Enumerable.Range(0, ReadersCount)
                .Select(_ => new BenchmarkEventReader(TimeSeriesFactory(), readerSettings))
                .ToList();

            writers = Enumerable.Range(0, WritersCount)
                .Select(_ => new BenchmarkEventWriter(TimeSeriesFactory(), writerSettings))
                .ToList();

            pool = new ReadersWritersPool(readers, writers);

            Database.ResetSchema();
        }

        [BenchmarkClassSetUp]
        public void ClassSetUp()
        {
            Database.SetUpSchema();
            Series = TimeSeriesFactory();
        }
        
        [BenchmarkClassTearDown]
        public void ClassTearDown()
        {
            Database.TearDownSchema();
        }
        
        [BenchmarkMethod(executionsCount:1, result:nameof(Result))]
        public void TimeSeries()
        {
            pool.Start();
            Thread.Sleep(5*1000);
            pool.Stop();

            CurrentIteration++;
        }

        public IBenchmarkingResult Result()
        {
            return new DatabaseBenchmarkingResult(readers, writers);
        }
    }
}