using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Benchmarks.Reflection;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;

namespace Benchmarks.Benchmarks
{
    [BenchmarkClass]
    public class SimpleSeriesReadAndWriteBenchmark : ReadAndWriteBenchmark
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new SimpleTimeSeries(controller.Table);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }

    [BenchmarkClass]
    public class SimpleSeriesReadOnlyBenchmark : ReadOnlyBenchmark
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new SimpleTimeSeries(controller.Table);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }

    [BenchmarkClass]
    public class SimpleSeriesWriteOnlyBenchmark : WriteOnlyBenchmark
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new SimpleTimeSeries(controller.Table);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }

    [BenchmarkClass]
    public class CasSeriesReadAndWriteBenchmark : ReadAndWriteBenchmark
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(controller.Table);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }

    [BenchmarkClass]
    public class CasSeriesReadOnlyBenchmark : ReadOnlyBenchmark
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(controller.Table);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }

    [BenchmarkClass]
    public class CasSeriesWriteOnlyBenchmark : WriteOnlyBenchmark
    {
        protected override IDatabaseController Database => controller;
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(controller.Table);
        private readonly SimpleTimeSeriesDatabaseController controller = new SimpleTimeSeriesDatabaseController();
    }

}
