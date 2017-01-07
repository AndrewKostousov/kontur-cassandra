using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class Promise<TResult>
    {
        [CanBeNull]
        public TResult Result
        {
            get
            {
                try
                {
                    return tcs.Task.Result;
                }
                catch (AggregateException e)
                {
                    if (e.InnerExceptions.Count == 1)
                        throw e.InnerExceptions[0];
                    throw;
                }
            }
        }

        public void SetResult([CanBeNull] TResult result)
        {
            tcs.SetResult(result);
        }

        public void SetError([NotNull] Exception exception)
        {
            tcs.SetException(exception);
        }

        public override string ToString()
        {
            return $"TaskStatus: {tcs.Task.Status}, Result: {(tcs.Task.IsCompleted ? Convert.ToString(Result) : "<NOT COMPLETED>")}";
        }

        private readonly TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
    }
}