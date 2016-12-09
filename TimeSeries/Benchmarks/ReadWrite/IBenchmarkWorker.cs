using System;
using System.Collections.Generic;

namespace Benchmarks.ReadWrite
{
    interface IBenchmarkWorker
    {
        List<TimeSpan> Latency { get; }
    }
}