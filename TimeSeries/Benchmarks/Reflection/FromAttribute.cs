using System;

namespace Benchmarks.Benchmarks
{
    class FromAttribute : Attribute
    {
        public Type Attribute { get; }

        public FromAttribute(Type attribute)
        {
            Attribute = attribute;
        }
    }
}