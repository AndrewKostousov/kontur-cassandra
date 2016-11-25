using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Benchmarks
{
    abstract class BindingAttribute : Attribute
    {
        protected static readonly object[] noArgs = new object[0];

        public virtual void Bind(object source, MethodInfo method, object target, PropertyInfo property)
        {
            if (method.ReturnType == typeof(void))
                property.SetValue(target, CreateAction(source, method));
            else
                property.SetValue(target, CreateFunc(source, method));
        }

        protected Action CreateAction(object source, MethodInfo method)
        {
            return () => method.Invoke(source, noArgs);
        }

        protected Delegate CreateFunc(object source, MethodInfo method)
        {
            return Expression.Lambda(Expression.Call(Expression.Constant(source), method)).Compile();
        }
    }
}