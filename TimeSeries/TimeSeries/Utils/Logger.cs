using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries.Model
{
    static class Logger
    {
        private const string logfile = "log.txt";
        private static readonly object LockObject = new object();

        public static void Log(string message)
        {
            lock (LockObject)
                File.AppendAllText(logfile, $"[{DateTime.Now}] - {message}" + Environment.NewLine);
        }

        public static void Log(Exception exception)
        {
            if (exception.InnerException != null)
                Log(exception.InnerException);

            Log(exception.ToString());
        }
    }
}
