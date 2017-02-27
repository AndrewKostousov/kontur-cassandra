using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Benchmarks.Benchmarks;

namespace Benchmarks.Reflection
{
    class BenchmarkFinder
    {
        public IEnumerable<BenchmarksFixture> GetBenchmarks(Assembly assembly)
        {
            return GetSubclassesOf<BenchmarksFixture>(assembly)
                .Select(Activator.CreateInstance)
                .Cast<BenchmarksFixture>();
        }

        IEnumerable<Type> GetSubclassesOf<TParent>(Assembly assembly)
        {
            return assembly.ExportedTypes
                .Where(t => typeof(TParent).IsAssignableFrom(t))
                .Where(x => !x.IsAbstract && !x.IsInterface);
        }
    }
}