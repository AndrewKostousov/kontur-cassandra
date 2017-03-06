using System.Collections.Generic;
using System.Linq;
using Benchmarks.Benchmarks;

namespace Benchmarks.Runners
{
    class BenchmarksRunner
    {
        private readonly List<BenchmarksLogger> loggers;

        public BenchmarksRunner(params BenchmarksLogger[] loggers)
        {
            this.loggers = loggers.ToList();
        }

        public void RunAll(IEnumerable<BenchmarksFixture> benchmarks)
        {
            foreach (var fixture in benchmarks)
                RunSingleBenchmark(fixture);

            loggers.ForEach(lgr => lgr.LogAllFixturesFinished());
        }

        private void RunSingleBenchmark(BenchmarksFixture fixture)
        {
            fixture.BenchmarkSetup += b => loggers.ForEach(lgr => lgr.LogBenchmarkSetup(fixture, b));

            fixture.BenchmarkStarted += b => loggers.ForEach(lgr => lgr.LogBenchmarkStarted(fixture, b));

            fixture.BenchmarkFinished += (b, r) => loggers.ForEach(lgr => lgr.LogBenchmarkFinished(fixture, b, r.CreateReport()));

            fixture.BenchmarkTeardown += b => loggers.ForEach(lgr => lgr.LogBenchmarkTearDown(fixture, b));

            foreach (var logger in loggers)
                logger.LogFixtureBegan(fixture);

            fixture.Run();
        }
    }
}