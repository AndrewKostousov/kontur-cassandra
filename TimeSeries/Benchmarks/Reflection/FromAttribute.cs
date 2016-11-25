using System;

namespace Benchmarks.Benchmarks
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    class FromAttribute : Attribute
    {
        public Type Attribute { get; }

        public FromAttribute(Type attribute)
        {
            Attribute = attribute;
        }
    }
}