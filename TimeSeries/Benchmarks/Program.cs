using System.Reflection;
using Benchmarks.Reflection;
using Benchmarks.Runners;

namespace Benchmarks
{
    static class Program
    {
        static void Main(string[] args)
        {
            var benchmarks = new BenchmarkFinder().GetBenchmarks(Assembly.GetExecutingAssembly());
            new ConsoleBenchmarkRunner().RunAll(benchmarks);
        }
    }
}
