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
        protected override ITimeSeries TimeSeriesFactory() => new TimeSeries(Database.Table);
    }

    [BenchmarkClass]
    public class SimpleSeriesReadOnlyBenchmark : ReadOnlyBenchmark
    {
        protected override ITimeSeries TimeSeriesFactory() => new TimeSeries(Database.Table);
    }

    [BenchmarkClass]
    public class SimpleSeriesWriteOnlyBenchmark : WriteOnlyBenchmark
    {
        protected override ITimeSeries TimeSeriesFactory() => new TimeSeries(Database.Table);
    }

    [BenchmarkClass]
    public class CasSeriesReadAndWriteBenchmark : ReadAndWriteBenchmark
    {
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(Database.Table);
    }

    [BenchmarkClass]
    public class CasSeriesReadOnlyBenchmark : ReadOnlyBenchmark
    {
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(Database.Table);
    }

    [BenchmarkClass]
    public class CasSeriesWriteOnlyBenchmark : WriteOnlyBenchmark
    {
        protected override ITimeSeries TimeSeriesFactory() => new CasTimeSeries(Database.Table);
    }

}
