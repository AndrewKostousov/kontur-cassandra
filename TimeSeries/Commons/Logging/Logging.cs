using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using log4net;
using log4net.Config;

namespace Commons.Logging
{
    public static class Logging
    {
        public static void SetUpForTests([NotNull] string logName = "AllTests")
        {
            if (initialized)
                return;
            SetUpLog4Net(logName);
            RegisterUnhandledExceptionsHandlers(Log.For("UnhandledExceptions"));
            initialized = true;
        }

        private static void SetUpLog4Net([NotNull] string logName)
        {
            GlobalContext.Properties["LogDirectory"] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            GlobalContext.Properties["LogFileName"] = $"{logName}-{DateTime.Now:yyyy-MM-dd-HH.mm.ss}.log";
            XmlConfigurator.Configure(GetConfigStream());
        }

        [CanBeNull]
        private static Stream GetConfigStream()
        {
            var thisType = typeof(Logging);
            var configResourceName = $"{(IsExecutionViaTeamCity() ? "tc" : "local")}.test.log4net.config";
            return thisType.Assembly.GetManifestResourceStream(thisType, configResourceName);
        }

        private static bool IsExecutionViaTeamCity()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION"));
        }

        private static void RegisterUnhandledExceptionsHandlers([NotNull] ILog logger)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => { logger.Fatal("Unhandled exception in current AppDomain", (Exception)args.ExceptionObject); };
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                logger.Fatal("Unobserved TaskException", args.Exception);
                args.SetObserved();
            };
        }

        private static bool initialized;
    }
}