using System;

namespace EdiTimeline
{
    public static class ChainedNullCheckExtensions
    {
        public static TResult With<TInput, TResult>(this TInput o, Func<TInput, TResult> evaluator)
            where TResult : class where TInput : class
        {
            return o == null ? null : evaluator(o);
        }

        public static TResult? With<TInput, TResult>(this TInput o, Func<TInput, TResult?> evaluator)
            where TResult : struct where TInput : class
        {
            return o == null ? null : evaluator(o);
        }

        public static TResult Return<TInput, TResult>(this TInput o, Func<TInput, TResult> evaluator, TResult failureValue) where TInput : class
        {
            return o == null ? failureValue : evaluator(o);
        }

        public static TResult Return<TInput, TResult>(this TInput? o, Func<TInput?, TResult> evaluator, TResult failureValue) where TInput : struct
        {
            return o == null ? failureValue : evaluator(o);
        }

        public static TResult Return<TInput, TResult>(this TInput o, Func<TInput, TResult?> evaluator, TResult failureValue) where TInput : class where TResult : struct
        {
            return o == null ? failureValue : (evaluator(o) ?? failureValue);
        }

        public static TResult? Return<TInput, TResult>(this TInput o, Func<TInput, TResult> evaluator, TResult? failureValue) where TInput : class where TResult : struct
        {
            return o == null ? failureValue : evaluator(o);
        }
    }
}