using System.Collections.Generic;
using Benchmarks.Results;

namespace Benchmarks.ReadWrite
{
    interface IBenchmarkWorker
    {
        List<Measurement> Measurements { get; }
    }
}