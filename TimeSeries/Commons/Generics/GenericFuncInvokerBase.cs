using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace SKBKontur.Catalogue.Generics
{
    public class GenericFuncInvokerBase
    {
        protected GenericFuncInvokerBase(Expression bodyExpression)
        {
            genericMethodInfo = ((MethodCallExpression)bodyExpression).Method.GetGenericMethodDefinition();
        }

        protected MethodInfo GetMethodInfo(Type genericParameter)
        {
            return methodInfos.GetOrAdd(genericParameter, MakeMethodInfo);
        }

        private readonly MethodInfo genericMethodInfo;

        private MethodInfo MakeMethodInfo(Type genericParameter)
        {
            return genericMethodInfo.MakeGenericMethod(genericParameter);
        }

        private readonly ConcurrentDictionary<Type, MethodInfo> methodInfos = new ConcurrentDictionary<Type, MethodInfo>();
    }
}