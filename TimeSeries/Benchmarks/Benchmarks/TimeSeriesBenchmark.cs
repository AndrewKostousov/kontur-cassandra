using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Benchmarks.ReadWrite;
using Benchmarks.Results;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using Commons.TimeBasedUuid;

namespace Benchmarks.Benchmarks
{
    class TimeSeriesBenchmark : IBenchmark
    {
        public string Name { get; }

        private readonly TimeSeriesBenchmarkSettings settings;
        private readonly IDatabaseController maintenanceDatabase;
        private readonly ITimeSeries maintenanceSeries;
        private readonly List<ITimeSeries> readers;
        private readonly List<ITimeSeries> writers;

        private ReadersWritersPool<BenchmarkEventReader, BenchmarkEventWriter> pool;

        public TimeSeriesBenchmark(string name, TimeSeriesBenchmarkSettings settings,
            IDatabaseController maintenanceDatabase, ITimeSeries maintenanceSeries,
            List<ITimeSeries> readers, List<ITimeSeries> writers)
        {
            Name = name;
            this.readers = readers;
            this.writers = writers;
            this.settings = settings;
            this.maintenanceSeries = maintenanceSeries;
            this.maintenanceDatabase = maintenanceDatabase;
        }

        public void SetUp()
        {
            maintenanceDatabase.ResetSchema();

            if (writers.Count == 0) FillEvents();

            var warmUpPool = CreateWarmUpPool();

            warmUpPool.Start();
            Thread.Sleep((int)settings.WarmUpDuration.TotalMilliseconds);
            warmUpPool.Stop();

            pool = ConvertToBenchmarking(warmUpPool);
        }

        public IBenchmarkingResult Run()
        {
            pool.Start();
            Thread.Sleep((int)settings.BenchmarkDuration.TotalMilliseconds);
            pool.Stop();

            return new DatabaseBenchmarkingResult(pool.Readers, pool.Writers);
        }

        public void TearDown() { }

        private void FillEvents()
        {
            Enumerable.Range(0, 4).AsParallel().ForAll(x =>
            {
                for (var i = 0; i < settings.PreloadedEventsCount / 4; ++i)
                {
                    var id = TimeGuid.NowGuid();
                    maintenanceSeries.WriteWithoutSync(new Event(id, new EventProto()));
                }
            });
        }

        private ReadersWritersPool<IEventReader, IEventWriter> CreateWarmUpPool()
        {
            return new ReadersWritersPool<IEventReader, IEventWriter>(
                readers.Select(r => new EventReader(r, settings.ReaderSettings)).ToList(),
                writers.Select(w => new EventWriter(w, settings.WriterSettings)).ToList()
            );
        }

        private ReadersWritersPool<BenchmarkEventReader, BenchmarkEventWriter> ConvertToBenchmarking(ReadersWritersPool<IEventReader, IEventWriter> somePool)
        {
            return new ReadersWritersPool<BenchmarkEventReader, BenchmarkEventWriter>(
                somePool.Readers.Select(reader => new BenchmarkEventReader(reader)).ToList(),
                somePool.Writers.Select(writer => new BenchmarkEventWriter(writer)).ToList()
            );
        }
    }
}