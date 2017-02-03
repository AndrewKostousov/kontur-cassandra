using System;

namespace EdiTimeline.CassandraHelpers
{
    public static class ActionExtensions
    {
        public static Action<T1, T2> Concatenate<T1, T2>(this Action<T1, T2> firstAction, Action<T1, T2> nextAction)
        {
            return (x, t1) =>
            {
                firstAction(x, t1);
                nextAction(x, t1);
            };
        }

        public static Action<T1> Concatenate<T1>(this Action<T1> firstAction, Action<T1> nextAction)
        {
            return x =>
            {
                firstAction(x);
                nextAction(x);
            };
        }
    }
}