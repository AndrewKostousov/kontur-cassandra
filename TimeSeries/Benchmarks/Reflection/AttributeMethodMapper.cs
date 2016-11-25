using System;
using System.Linq;
using System.Reflection;
using Benchmarks.Benchmarks;
using Commons;

namespace Benchmarks
{
    class AttributeMethodMapper<TTarget>
        where TTarget : new()
    {
        public TTarget CreateInstance(object source)
        {
            var instance = Activator.CreateInstance<TTarget>();
            var markedProperties = typeof(TTarget).GetProperties()
                .Select(x => InfoAttributePair.Create(x, x.GetCustomAttribute<FromAttribute>()))
                .Where(x => x.Attribute != null);

            foreach (var property in markedProperties)
                BindMethodToProperty(source, instance, property.Info, property.Attribute);

            return instance;
        }

        private void BindMethodToProperty(object source, TTarget instance, PropertyInfo property, FromAttribute attribute)
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