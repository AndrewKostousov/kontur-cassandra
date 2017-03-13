using System.Reflection;
using Benchmarks.Reflection;
using Benchmarks.Runners;

namespace Benchmarks
{
    static class Program
    {
        static void Main()
        {
            var benchmarks = new BenchmarkFinder().GetBenchmarks(Assembly.GetExecutingAssembly());

            new BenchmarksRunner(
                new ConsoleBenchmarksLogger(), 
                new FileBenchmarksLogger(),
                new JsonBenchmarkLogger()
            ).RunAll(benchmarks);
        }
    }
}
