using System;
using System.Collections.Generic;
using System.IO;
using Benchmarks.Benchmarks;

namespace Benchmarks.Runners
{
    class ConsoleBenchmarkRunner : IBenchmarkRunner
    {
        readonly string separator = "\n".PadLeft(100, '=') + "\n";

        private string pathToLog = "log.txt";

        public void RunAll(IEnumerable<BenchmarksFixture> benchmarks)
        {
            File.WriteAllText(pathToLog, "");

            foreach (var fixture in benchmarks)
                RunSingleBenchmark(fixture);

            Console.WriteLine(separator + "All benchmarks done! Press any key to exit.");
            Console.ReadKey();
        }

        private void RunSingleBenchmark(BenchmarksFixture fixture)
        {
            Action<string> log = message =>
            {
                File.AppendAllText(pathToLog, message);
                Console.Write(message);
            };


            log(separator + $"Preparing fixture: {fixture.Name}\n\n");

            fixture.BenchmarkSetupStarted += b => log(separator + $"Setting up: {b.Name} ... ");
            fixture.IterationStarted += (b, i) => log($"Done!\nRunning benchmark: {b.Name} ... ");
            fixture.BenchmarkFinished += (b, r) => log($"Done!\n\n{r.CreateReport()}\n");

            fixture.Run();
        }
    }
}