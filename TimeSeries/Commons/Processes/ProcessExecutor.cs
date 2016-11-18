using System;
using System.Diagnostics;

using log4net;

namespace SKBKontur.Catalogue.Processes
{
    public class ProcessExecutor : IProcessExecutor
    {
        public void ExecuteProcess(string fileName, string arguments, string workingDirectory, int timeout, string toStdin = null)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();

            if(toStdin != null)
                SafeAction(() => process.StandardInput.WriteLine(toStdin), "Write input for process", true);

            process.WaitForExit(timeout);

            SafeAction(process.Kill, "Kill process", false);
            var exitCode = process.ExitCode;
            if(exitCode != 0)
                throw new ProcessExecutionException(exitCode);
        }

        private void SafeAction(Action action, string actionName, bool logExceptionAsError)
        {
            try
            {
                action();
            }
            catch(Exception e)
            {
                if (logExceptionAsError)
                    logger.Error(string.Format("Error while execute action '{0}'.", actionName), e);
                else
                    logger.Info(string.Format("Error while execute action '{0}'.", actionName), e);
            }
        }

        private readonly ILog logger = LogManager.GetLogger(typeof(ProcessExecutor));
    }
}