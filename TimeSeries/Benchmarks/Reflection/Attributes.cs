using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Benchmarks.Benchmarks;

namespace Benchmarks
{
    abstract class BindingAttribute : Attribute
    {
        protected static readonly object[] noArgs = new object[0];

        public virtual void Bind(object source, MethodInfo method, 
            object target, PropertyInfo property)
        {
            property.SetValue(target, new Action(() => method.Invoke(source, noArgs)));
        }
    }

    class BenchmarkClassAttribute : BindingAttribute
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

        public BenchmarkMethodAttribute(int executionsCount=100)
        {
            this.executionsCount = executionsCount;
        }

        public override void Bind(object source, MethodInfo method,
            object target, PropertyInfo property)
        {
            (property.GetValue(target) as List<Benchmark>)?
                .Add(new Benchmark(method.Name, executionsCount, 
                () => method.Invoke(source, noArgs)));
        }
    }

    class BenchmarkResultProviderAttribute : BindingAttribute
    {
        
    }
}
