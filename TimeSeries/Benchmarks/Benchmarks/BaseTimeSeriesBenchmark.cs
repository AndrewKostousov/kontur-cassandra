using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarks.Reflection;
using Cassandra;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Utils;

namespace Benchmarks.Benchmarks
{
    public abstract class BaseTimeSeriesBenchmark<TDatabaseController> : BenchmarksFixture
        where TDatabaseController : IDatabaseController, new ()
    {
        protected abstract ITimeSeries TimeSeriesFactory(TDatabaseController controller);

        private ITimeSeries maintenanceSeries;
        private TDatabaseController maintenanceDatabase;
        private TDatabaseController[] readersControllersPool;
        private TDatabaseController[] writersControllersPool;

        private static readonly int[] DefaultReadersWritersCount = { 0, 1, 2, 4, 8, 16, 32 };

        protected virtual int[] ReadersCountRange => DefaultReadersWritersCount;
        protected virtual int[] WritersCountRange => DefaultReadersWritersCount;

        protected virtual TimeSeriesBenchmarkSettings Settings => TimeSeriesBenchmarkSettings.Default();

        protected override void ClassSetUp()
        {
            maintenanceDatabase = new TDatabaseController();
            maintenanceDatabase.SetUpSchema();
            maintenanceSeries = TimeSeriesFactory(maintenanceDatabase);

            readersControllersPool = CreatePool(ReadersCountRange.Max());
            writersControllersPool = CreatePool(WritersCountRange.Max());
        }

        private TDatabaseController[] CreatePool(int poolSize)
        {
            var pool = Enumerable
                .Range(0, poolSize)
                .Select(_ => { Console.Write("."); return new TDatabaseController(); })
                .ToArray();

            Console.WriteLine();

            return pool;
        }

        protected override void ClassTearDown()
        {
            maintenanceDatabase.Dispose();

            foreach (var databaseController in readersControllersPool)
                databaseController.Dispose();

            foreach (var databaseController in writersControllersPool)
                databaseController.Dispose();
        }

        protected override IEnumerable<IBenchmark> GetBenchmarks()
        {
            return ReadersCountRange.Product(WritersCountRange, (i, j) => new {Readers = i, Writers = j})
                .Where(num => num.Writers != 0)
                .Select(num => new TimeSeriesBenchmark(CreateBenchmarkName(num.Readers, num.Writers),
                    Settings, maintenanceDatabase, maintenanceSeries, 
                    InitTimeSeries(num.Readers, readersControllersPool), InitTimeSeries(num.Writers, writersControllersPool)));
        }

        private String CreateBenchmarkName(int numReaders, int numWriters)
        {
            return $"{numReaders} reader{(numReaders == 1 ? "" : "s")}, {numWriters} writer{(numWriters == 1 ? "" : "s")}";
        }

        private List<ITimeSeries> InitTimeSeries(int count, TDatabaseController[] pool)
        {
            return pool.Take(count).Select(TimeSeriesFactory).ToList();
        }
    }
}