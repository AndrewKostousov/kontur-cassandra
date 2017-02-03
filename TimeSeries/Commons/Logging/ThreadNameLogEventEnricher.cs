using System.Threading;
using JetBrains.Annotations;
using Serilog.Core;
using Serilog.Events;

namespace Commons.Logging
{
    public class ThreadNameLogEventEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadName", GetCurrentThreadName()));
        }

        [NotNull]
        private static string GetCurrentThreadName()
        {
            var threadName = Thread.CurrentThread.Name;
            if (string.IsNullOrWhiteSpace(threadName))
                threadName = Thread.CurrentThread.ManagedThreadId.ToString();
            return threadName;
        }
    }
}