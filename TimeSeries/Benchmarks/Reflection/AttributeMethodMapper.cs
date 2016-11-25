using System;
using System.Linq;
using System.Reflection;
using Benchmarks.Benchmarks;
using Commons;

namespace Benchmarks.Reflection
{
    class AttributeMethodMapper
    {
        public TTarget ApplyMapping<TTarget>(TTarget instance, object source)
        {
            var markedProperties = typeof(TTarget).GetProperties()
                .SelectMany(x => x.GetCustomAttributes<FromAttribute>().Select(a => InfoAttributePair.Create(x, a)));

            foreach (var property in markedProperties)
                BindMethodToProperty(source, instance, property.Info, property.Attribute);

            return instance;
        }

        private void BindMethodToProperty<TTarget>(object source, TTarget instance, 
            PropertyInfo property, FromAttribute attribute)
        {
            var markedMethods = source.GetType().GetMethods()
                .Select(x => InfoAttributePair.Create(x, x.GetCustomAttribute(attribute.Attribute)))
                .Where(x => x.Attribute != null);

            foreach (var method in markedMethods)
            {
                var binder = method.Attribute as BindingAttribute;

                if (binder == null)
                    throw new InvalidProgramStateException("Attribute-marker sould be subclass of BindingAttribute");

                binder.Bind(source, method.Info, instance, property);
            }

        }
    }
}