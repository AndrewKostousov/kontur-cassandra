using System;
using System.Linq.Expressions;

namespace SKBKontur.Catalogue.Generics
{
    public class GenericFuncInvoker<TResult> : GenericFuncInvokerBase
    {
        public GenericFuncInvoker(Expression<Func<TResult>> invokeMethodExpression)
            : base(invokeMethodExpression.Body)
        {
        }

        public TResult Invoke(object obj, Type genericParameter)
        {
            return (TResult)GetMethodInfo(genericParameter).Invoke(obj, new object[0]);
        }
    }

    public class GenericFuncInvoker<TP1, TP2, TP3, TResult> : GenericFuncInvokerBase
    {
        public GenericFuncInvoker(Expression<Func<TP1, TP2, TP3, TResult>> invokeMethodExpression)
            : base(invokeMethodExpression.Body)
        {
        }

        public TResult Invoke(object obj, Type genericParameter, TP1 p1, TP2 p2, TP3 p3)
        {
            return (TResult)GetMethodInfo(genericParameter).Invoke(obj, new object[] {p1, p2, p3});
        }
    }

    public class GenericActionInvoker<TP1> : GenericFuncInvokerBase
    {
        public GenericActionInvoker(Expression<Action<TP1>> invokeMethodExpression)
            : base(invokeMethodExpression.Body)
        {
        }

        public void Invoke(object obj, Type genericParameter, TP1 p1)
        {
            GetMethodInfo(genericParameter).Invoke(obj, new object[] {p1});
        }
    }   
    
    public class GenericLocalActionInvoker<TP1> : GenericFuncInvokerBase
    {
        private readonly object self;

        public GenericLocalActionInvoker(Expression<Action<TP1>> invokeMethodExpression, object self)
            : base(invokeMethodExpression.Body)
        {
            this.self = self;
        }

        public void Invoke(Type genericParameter, TP1 p1)
        {
            GetMethodInfo(genericParameter).Invoke(self, new object[] {p1});
        }
    }
}