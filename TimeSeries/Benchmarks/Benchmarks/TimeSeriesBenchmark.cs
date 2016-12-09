using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Benchmarks.ReadWrite;
using Benchmarks.Reflection;
using Benchmarks.Results;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using Commons.TimeBasedUuid;

namespace Benchmarks.Benchmarks
{
    [BenchmarkClass]
    public abstract class TimeSeriesBenchmark
    {
        private DatabaseWrapper database;
        protected TimeSeries Series;
        private ReadersWritersPool pool;

        private List<BenchmarkEventReader> readers;
        private List<BenchmarkEventWriter> writers;

        private readonly ReaderSettings readerSettings;
        private readonly WriterSettings writerSettings;

        private readonly int readersCount;
        private readonly int writersCount;

        protected TimeSeriesBenchmark(int readersCount, int writersCount)
        {
            this.readersCount = readersCount;
            this.writersCount = writersCount;

            readerSettings = new ReaderSettings();
            writerSettings = new WriterSettings();
        }

        [BenchmarkSetUp]
        public virtual void SetUp()
        {
            readers = Enumerable.Range(0, readersCount)
                .Select(_ => new BenchmarkEventReader(Series, readerSettings))
                .ToList();

            writers = Enumerable.Range(0, writersCount)
                .Select(_ => new BenchmarkEventWriter(Series, writerSettings))
                .ToList();

            pool = new ReadersWritersPool(readers, writers);

            database.Table.Truncate();
            Series.Write(new Event(TimeGuid.NowGuid()));
        }

        [BenchmarkClassSetUp]
        public void ClassSetUp()
        {
            database = new DatabaseWrapper("test");
            Series = new TimeSeries(database.Table);
            Series.Write(new Event(TimeGuid.NowGuid()));
        }
        
        [BenchmarkClassTearDown]
        public void ClassTearDown()
        {
            database.Dispose();
        }
        
        [BenchmarkMethod(executionsCount:1, result:nameof(Result))]
        public void TimeSeries()
        {
            pool.Start();
            Thread.Sleep(10000);
            pool.Stop();
        }

        public IBenchmarkingResult Result()
        {
            return new DatabaseBenchmarkingResult(readers, writers);
        }
    }
}