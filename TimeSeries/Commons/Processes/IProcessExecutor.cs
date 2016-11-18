namespace SKBKontur.Catalogue.Processes
{
    public interface IProcessExecutor
    {
        void ExecuteProcess(string fileName, string arguments, string workingDirectory, int timeout, string toStdin = null);
    }
}