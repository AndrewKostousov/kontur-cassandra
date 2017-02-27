using System;
using System.Collections.Generic;
using System.IO;
using Benchmarks.Benchmarks;

namespace Benchmarks.Runners
{
    class ConsoleBenchmarkRunner : IBenchmarkRunner
    {
        readonly string separator = "\r\n".PadLeft(100, '=');

        private string pathToLog = "results.txt";

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
            Console.Write(separator + $"\r\nPreparing fixture: {fixture.Name}\r\n\r\n{separator}");

            File.AppendAllText(pathToLog, $"{separator}\r\n   {fixture.Name}\r\n\r\n{separator}");

            fixture.BenchmarkSetupStarted += b => Console.Write($"\r\nSetting up: {b.Name} ... ");
            fixture.IterationStarted += (b, i) => Console.Write($"Done!\r\n\r\nRunning benchmark: {b.Name} ... ");

            fixture.BenchmarkFinished += (b, r) =>
            {
                var report = r.CreateReport();

                Console.Write($"Done!\r\n\r\n{report}");
                File.AppendAllText(pathToLog, "\r\n" + report);
            };

            fixture.Run();
        }
    }
}