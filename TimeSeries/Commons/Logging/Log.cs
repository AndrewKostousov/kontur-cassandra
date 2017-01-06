using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using log4net;

namespace Commons.Logging
{
    public static class Log
    {
        [NotNull]
        public static ILog For<T>()
        {
            return For(typeof(T));
        }

        [NotNull]
        public static ILog For<T>(T instance)
        {
            return For<T>();
        }

        [NotNull]
        public static ILog For([NotNull] Type type)
        {
            return GetLoggerByName(type.FullName);
        }

        [NotNull]
        public static ILog For([NotNull] string loggerName)
        {
            return GetLoggerByName(loggerName);
        }

        [NotNull]
        private static ILog GetLoggerByName([NotNull] string loggerName)
        {
            return LoggerRepository.GetLogger(loggerName);
        }

        private static class LoggerRepository
        {
            private static readonly ConcurrentDictionary<string, ILog> loggers = new ConcurrentDictionary<string, ILog>();

            [NotNull]
            public static ILog GetLogger([NotNull] string loggerName)
            {
                return loggers.GetOrAdd(loggerName, t => LogManager.GetLogger(loggerName));
            }
        }
    }
}