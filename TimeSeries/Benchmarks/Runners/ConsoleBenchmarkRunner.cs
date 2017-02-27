using System;
using System.Collections.Generic;
using System.IO;
using Benchmarks.Benchmarks;

namespace Benchmarks.Runners
{
    class ConsoleBenchmarkRunner : IBenchmarkRunner
    {
        private static readonly string NewLine = Environment.NewLine;
        private readonly string separator = NewLine.PadLeft(100, '=');
        private readonly string pathToLog = "results.txt";

        public void RunAll(IEnumerable<BenchmarksFixture> benchmarks)
        {
            File.WriteAllText(pathToLog, "");

            foreach (var fixture in benchmarks)
                RunSingleBenchmark(fixture);

            Console.WriteLine($"{separator}All benchmarks done! Press any key to exit.");
            Console.ReadKey();
        }

        private void RunSingleBenchmark(BenchmarksFixture fixture)
        {
            Func<string, string> makeRectangle = s => $"{separator}{NewLine}{s}{NewLine}{NewLine}{separator}";

            Console.Write(makeRectangle($"Running fixture: {fixture.Name}"));
            File.AppendAllText(pathToLog, makeRectangle($"   {fixture.Name}"));

            fixture.BenchmarkSetup += b => Console.Write($"{NewLine}Setting up: {b.Name} ... ");

            fixture.BenchmarkStarted += b =>
            {
                Console.Write($"Done!{NewLine}{NewLine}Running benchmark: {b.Name} ... ");
                File.AppendAllText(pathToLog, $"{NewLine}== {b.Name} ".PadRight(100, '=') + NewLine);
            };

            fixture.BenchmarkFinished += (b, r) =>
            {
                var report = r.CreateReport();

                Console.Write($"Done!{NewLine}{NewLine}{report}");
                File.AppendAllText(pathToLog, NewLine + report);
            };

            fixture.Run();
        }
    }
}