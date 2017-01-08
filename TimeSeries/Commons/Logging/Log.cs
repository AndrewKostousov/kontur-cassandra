using System;
using JetBrains.Annotations;
using Serilog;

namespace Commons.Logging
{
    public static class Log
    {
        [NotNull]
        public static ILogger For<T>()
        {
            return For(typeof(T));
        }

        [NotNull]
        public static ILogger For<T>(T instance)
        {
            return For<T>();
        }

        [NotNull]
        public static ILogger For([NotNull] Type type)
        {
            return Logging.GetLoggerByName(type.FullName);
        }

        [NotNull]
        public static ILogger For([NotNull] string loggerName)
        {
            return Logging.GetLoggerByName(loggerName);
        }
    }
}