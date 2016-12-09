using System.Collections.Generic;
using Benchmarks.Benchmarks;

namespace Benchmarks.Runners
{
    interface IBenchmarkRunner
    {
        void RunAll(IEnumerable<BenchmarksFixture> benchmarks);
    }
}
