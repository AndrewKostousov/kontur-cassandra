using System;

namespace SKBKontur.Catalogue.Processes
{
    public class ProcessExecutionException : Exception
    {
        public ProcessExecutionException(int exitCode)
            : base(string.Format("Execution failed with exitCode={0}", exitCode))
        {
            ProcessExitCode = exitCode;
        }

        public int ProcessExitCode { get; private set; }
    }
}