using JetBrains.Annotations;
using Serilog.Core;
using Serilog.Events;

namespace Commons.Logging
{
    public class LoggerNameLogEventEnricher : ILogEventEnricher
    {
        public LoggerNameLogEventEnricher([NotNull] string loggerName)
        {
            this.loggerName = loggerName;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("LoggerName", loggerName));
        }

        private readonly string loggerName;
    }
}