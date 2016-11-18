using System;
using System.Runtime.Caching;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Objects
{
    public class InMemoryCache
    {
        public InMemoryCache(TimeSpan cacheItemTtl, bool slidingExpiration)
        {
            cache = new InMemoryCache<DummyExceptionWhichIsNeverThrown>(cacheItemTtl, slidingExpiration);
        }

        [NotNull]
        public TResult GetResult<TResult>([NotNull] string cacheKey, [NotNull] Func<TResult> getResult) where TResult : class
        {
            return cache.GetResult(cacheKey, getResult);
        }

        private readonly InMemoryCache<DummyExceptionWhichIsNeverThrown> cache;

        private class DummyExceptionWhichIsNeverThrown : Exception
        {
        }
    }

    public class InMemoryCache<TError>
        where TError : Exception
    {
        public InMemoryCache(TimeSpan cacheItemTtl, bool slidingExpiration)
        {
            this.cacheItemTtl = cacheItemTtl;
            this.slidingExpiration = slidingExpiration;
        }

        [NotNull]
        public TResult GetResult<TResult>([NotNull] string cacheKey, [NotNull] Func<TResult> getResult) where TResult : class
        {
            return cache.GetValueUsingCache(cacheKey, cacheItemTtl, () => ResultHolder<TResult>.Success(getResult()), slidingExpiration).GetResult();
        }

        [NotNull]
        public TResult GetResultOrThrow<TResult>([NotNull] string cacheKey, [NotNull] Func<TResult> getResult) where TResult : class
        {
            var resultHolder = cache.GetValueUsingCache(cacheKey, cacheItemTtl, () =>
                {
                    try
                    {
                        return ResultHolder<TResult>.Success(getResult());
                    }
                    catch(TError e)
                    {
                        return ResultHolder<TResult>.Failure(e);
                    }
                }, slidingExpiration);
            if(resultHolder.ErrorOccured)
                throw resultHolder.GetException();
            return resultHolder.GetResult();
        }

        private readonly MemoryCache cache = MemoryCache.Default;
        private readonly TimeSpan cacheItemTtl;
        private readonly bool slidingExpiration;

        private class ResultHolder<TResult>
            where TResult : class
        {
            private ResultHolder([CanBeNull] TResult result, [CanBeNull] TError exception)
            {
                this.result = result;
                this.exception = exception;
            }

            [NotNull]
            public static ResultHolder<TResult> Success([NotNull] TResult result)
            {
                return new ResultHolder<TResult>(result, null);
            }

            [NotNull]
            public static ResultHolder<TResult> Failure([NotNull] TError exception)
            {
                return new ResultHolder<TResult>(null, exception);
            }

            [NotNull]
            public TResult GetResult()
            {
                if(result == null)
                    throw new InvalidProgramStateException("Result is expected to be filled");
                return result;
            }

            [NotNull]
            public TError GetException()
            {
                if(exception == null)
                    throw new InvalidProgramStateException("Exception is expected to be filled");
                return exception;
            }

            public override string ToString()
            {
                return ErrorOccured ? string.Format("ERROR: {0}", GetException().Message) : "SUCCESS";
            }

            public bool ErrorOccured { get { return exception != null; } }
            private readonly TResult result;
            private readonly TError exception;
        }
    }
}