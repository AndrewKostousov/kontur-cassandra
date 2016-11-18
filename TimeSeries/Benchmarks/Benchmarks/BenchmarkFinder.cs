using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Benchmarks.Benchmarks
{
    class Extractor
    {
        public IEnumerable<T> Extract<T>(Assembly assembly)
        {
            return GetSubclassesOf<T>(assembly)
                .Select(t => (T) Activator.CreateInstance(t));
        }

        IEnumerable<Type> GetSubclassesOf<T>(Assembly assembly)
        {
            return assembly.ExportedTypes
                .Where(t => typeof(T).IsAssignableFrom(t))
                .Where(x => !x.IsAbstract && !x.IsInterface)
                .ToList();
        }
    }
}