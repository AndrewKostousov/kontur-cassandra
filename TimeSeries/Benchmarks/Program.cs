using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Metrics;
using Metrics.MetricData;
using Metrics.Reporters;

namespace Benchmarks
{
    static class Program
    {
        static void Main(string[] args)
        {
            new ConsoleBenchmarkRunner().RunAll(Assembly.GetExecutingAssembly());
        }
    }
}
