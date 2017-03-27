using Cassandra;

namespace CassandraTimeSeries.Utils
{
    public static class DriverExceptionExtensions
    {
        public static bool IsCritical(this DriverException ex)
        {
            return ex is QueryValidationException || ex is RequestInvalidException || ex is InvalidTypeException;
        }

    }
}
