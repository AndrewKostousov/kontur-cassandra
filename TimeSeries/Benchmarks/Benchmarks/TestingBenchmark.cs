using System;
using Benchmarks.Reflection;
using Benchmarks.Results;

namespace Benchmarks.Benchmarks
{
    //[BenchmarkClass]
    public class TestingBenchmark
    {
        [BenchmarkClassSetUp]
        public void ClassSetUp()
        {
            Console.WriteLine($"{nameof(ClassSetUp)}");
        }

        [BenchmarkSetUp]
        public void SetUp()
        {
            Console.WriteLine($"{nameof(SetUp)}");
        }

        [BenchmarkMethod(1)]
        public void First()
        {
            Console.WriteLine($"{nameof(First)}");
        }

        [BenchmarkMethod(1, nameof(CountResult))]
        public void Second()
        {
            Console.WriteLine($"{nameof(Second)}");
        }

        [BenchmarkTearDown]
        public void TearDown()
        {
            Console.WriteLine($"{nameof(TearDown)}");
        }

        [BenchmarkClassTearDown]
        public void ClassTearDown()
        {
            Console.WriteLine($"{nameof(ClassTearDown)}");
        }
        
        public IBenchmarkingResult CountResult()
        {
            Console.WriteLine($"{nameof(CountResult)}");
            return new BenchmarkingResult(TimeSpan.FromSeconds(42));
        }
    }
}