using System;
using Cassandra;

namespace CassandraTimeSeries.Utils
{
    public static class ExceptionExtensions
    {
        public static bool IsCritical(this Exception ex)
        {
            return (ex is QueryValidationException || ex is RequestInvalidException || ex is InvalidTypeException)
                && !(ex is QueryExecutionException);
        }
    }
}
