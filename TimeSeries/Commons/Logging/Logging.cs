using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Serilog;

namespace Commons.Logging
{
    public static class Logging
    {
        [NotNull]
        public static ILogger GetLoggerByName([NotNull] string loggerName)
        {
            return loggers.GetOrAdd(loggerName, t => GetBasicConfiguration().Enrich.With(new LoggerNameLogEventEnricher(loggerName)).CreateLogger());
        }

        public static void SetUp()
        {
            logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"{DateTime.Now:yyyy-MM-dd_HH.mm.ss}.log");
            Serilog.Log.Logger = GetBasicConfiguration().CreateLogger();
            RegisterUnhandledExceptionsHandlers(Log.For("UnhandledExceptions"));
        }

        public static void TearDown()
        {
            foreach (var logger in loggers.Values.Select(x => x as IDisposable).Where(x => x != null))
                logger.Dispose();
            Serilog.Log.CloseAndFlush();
        }

        [NotNull]
        private static LoggerConfiguration GetBasicConfiguration()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.With(new ThreadNameLogEventEnricher())
                .WriteTo.LiterateConsole(outputTemplate: "{Timestamp:HH:mm:ss.fff} {Level} [{ThreadName}] {Message}{NewLine}{Exception}")
                .WriteTo.File(path: logFilePath, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fffzzz} {Level:u4} [{ThreadName}] [{LoggerName}] {Message}{NewLine}{Exception}", shared: true);
        }

        private static void RegisterUnhandledExceptionsHandlers([NotNull] ILogger logger)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => { logger.Fatal((Exception)args.ExceptionObject, "Unhandled exception in current AppDomain"); };
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                logger.Fatal(args.Exception, "Unobserved TaskException");
                args.SetObserved();
            };
        }

        private static string logFilePath;
        private static readonly ConcurrentDictionary<string, ILogger> loggers = new ConcurrentDictionary<string, ILogger>();
    }
}