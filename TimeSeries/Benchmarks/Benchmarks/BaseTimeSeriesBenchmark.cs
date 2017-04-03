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
using Commons.TimeBasedUuid;

namespace Benchmarks.Benchmarks
{
    public abstract class BaseTimeSeriesBenchmark<TDatabaseController> : BenchmarksFixture
        where TDatabaseController : IDatabaseController, new ()
    {
        protected abstract ITimeSeries TimeSeriesFactory(TDatabaseController controller);

        private ITimeSeries series;

        protected virtual ReaderSettings ReaderSettings { get; } = new ReaderSettings();
        protected virtual WriterSettings WriterSettings { get; } = new WriterSettings();

        private static readonly int[] DefaultReadersWritersCount = { 0, 1, 2, 4, 8, 16, 32, 64 };

        private readonly IDatabaseController database = new TDatabaseController();

        protected virtual int[] ReadersCountRange => new[] {0, 1, 4};
        protected virtual int[] WritersCountRange => DefaultReadersWritersCount;

        protected virtual int PreloadedEventsCount { get; } = 5000;

        protected override void ClassSetUp()
        {
            database.SetUpSchema();
            series = TimeSeriesFactory(new TDatabaseController());
        }
        
        protected override void ClassTearDown()
        {
            database.TearDownSchema();
        }

        protected override IEnumerable<Benchmark> GetBenchmarks()
        {
            return ReadersCountRange.Product(WritersCountRange, (i, j) => new {Readers = i, Writers = j})
                .Where(x => x.Writers != 0 || x.Readers != 0)
                .Select(
                    num => new Benchmark($"{num.Readers} reader{(num.Readers==1?"":"s")}, {num.Writers} writer{(num.Writers==1?"":"s")}",
                        () =>
                        {
                            SetUp(num.Readers, num.Writers);
                            if (num.Writers == 0) FillEvents();
                        }, 
                        RunGenericBenchmark)
                );
        }

        private ReadersWritersPool<BenchmarkEventReader, BenchmarkEventWriter> pool;

        private void SetUp(int readersCount, int writersCount)
        {
            database.ResetSchema();
            pool = InitReadersWritersPool(readersCount, writersCount);
        }

        private void FillEvents()
        {
            for (var i = 0; i < PreloadedEventsCount; ++i)
            {
                var id = TimeGuid.NowGuid();
                series.WriteWithoutSync(new Event(id, new EventProto()));
            }
        }

        private ReadersWritersPool<BenchmarkEventReader, BenchmarkEventWriter> InitReadersWritersPool(int readersCount, int writersCount)
        {
            var readers = Enumerable.Range(0, readersCount).Select(_ => new BenchmarkEventReader(TimeSeriesFactory(new TDatabaseController()), ReaderSettings)).ToList();
            var writers = Enumerable.Range(0, writersCount).Select(_ => new BenchmarkEventWriter(TimeSeriesFactory(new TDatabaseController()), WriterSettings)).ToList();
            return new ReadersWritersPool<BenchmarkEventReader, BenchmarkEventWriter>(readers, writers);
        }

        private IBenchmarkingResult RunGenericBenchmark()
        {
            pool.Start();
            Thread.Sleep(5 * 1000);
            pool.Stop();

            return new DatabaseBenchmarkingResult(pool.Readers, pool.Writers);
        }
    }
}