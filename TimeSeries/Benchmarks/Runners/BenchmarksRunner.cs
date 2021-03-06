﻿using System.Collections.Generic;
using System.Linq;
using Benchmarks.Benchmarks;
using Commons.Logging;

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
            Logging.SetUp();

            foreach (var fixture in benchmarks)
                RunSingleBenchmark(fixture);

            loggers.ForEach(lgr => lgr.LogAllFixturesFinished());
        }

        private void RunSingleBenchmark(BenchmarksFixture fixture)
        {
            fixture.BenchmarkSetup += b => loggers.ForEach(lgr => lgr.LogBenchmarkSetup(fixture, b));

            fixture.BenchmarkStarted += b => loggers.ForEach(lgr => lgr.LogBenchmarkStarted(fixture, b));

            fixture.BenchmarkFinished += (b, r) => loggers.ForEach(lgr => lgr.LogBenchmarkFinished(fixture, b, r));

            fixture.BenchmarkTeardown += b => loggers.ForEach(lgr => lgr.LogBenchmarkTearDown(fixture, b));

            fixture.OnClassSetup += () => loggers.ForEach(lgr => lgr.LogFixtureSetup(fixture));

            fixture.OnClassTearDown += () => loggers.ForEach(lgr => lgr.LogFixtureTearDown(fixture));

            foreach (var logger in loggers)
                logger.LogFixtureBegan(fixture);

            fixture.Run();
        }
    }
}