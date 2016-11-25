using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Benchmarks.Benchmarks;

namespace Benchmarks
{
    [AttributeUsage(AttributeTargets.Class)]
    class BenchmarkClassAttribute : Attribute
    {
    }

    class BenchmarkSetUpAttribute : BindingAttribute
    {
    }
    
    class BenchmarkTearDownAttribute : BindingAttribute
    {
    }
    
    class BenchmarkClassSetUpAttribute : BindingAttribute
    {
    }
    
    class BenchmarkClassTearDownAttribute : BindingAttribute
    {
    }

    class BenchmarkMethodAttribute : BindingAttribute
    {
        private readonly int executionsCount;
        private readonly string resultMethodName;

        public BenchmarkMethodAttribute(int executionsCount=100, string result=null)
        {
            this.executionsCount = executionsCount;
            this.resultMethodName = result;
        }

        public override void Bind(object source, MethodInfo method, object target, PropertyInfo property)
        {
            Benchmark benchmark;

            if (resultMethodName != null)
            {
                var resultMethod = source.GetType().GetMethods()
                    .Single(m => m.Name == resultMethodName);

                benchmark = new Benchmark(method.Name, executionsCount, () => method.Invoke(source, noArgs),
                    (Func<IBenchmarkingResult>)CreateFunc(source, resultMethod));
            }
            else
            {
                benchmark = new Benchmark(method.Name, executionsCount, () => method.Invoke(source, noArgs));
            }

            (property.GetValue(target) as List<Benchmark>)?.Add(benchmark);
        }
    }
}
