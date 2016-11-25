using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarks.Benchmarks
{
    class BenchmarkingResult : IBenchmarkingResult
    {
        public TimeSpan AverageExecutionTime { get; }

        public BenchmarkingResult(TimeSpan averageExecutionTime)
        {
            AverageExecutionTime = averageExecutionTime;
        }

        public string CreateReport()
        {
            return $"Average execution time: {AverageExecutionTime.Milliseconds} ms";
        }
    }
}
