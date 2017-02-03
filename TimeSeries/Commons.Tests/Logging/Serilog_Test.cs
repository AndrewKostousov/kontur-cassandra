using System;
using Commons.Logging;
using NUnit.Framework;

namespace Commons.Tests.Logging
{
    [TestFixture]
    public class Serilog_Test
    {
        [OneTimeSetUp]
        public void BeforeAllTests()
        {
            Commons.Logging.Logging.SetUp();
        }

        [OneTimeTearDown]
        public void AfterAllTests()
        {
            Commons.Logging.Logging.TearDown();
        }

        [Test]
        public void DefaultLogger()
        {
            Serilog.Log.Debug("Debug message");
            Serilog.Log.Information("Info message {i42} {@obj42}", 42, new { x = 42 });
            Serilog.Log.Warning("Warn message {i42} {$array}", 42, new []{42});
            Serilog.Log.Error(new InvalidProgramStateException("IPSE"), "Error message {i42} {@obj42}", 42, new { x = 42 });
            Serilog.Log.Fatal(new Exception("Exception"), "Fatal message {0} {@1}", 42, new { x = 42 });
        }

        [Test]
        public void NamedLogger()
        {
            Log.For<int>().Debug("Debug message <int>");
            Log.For(new {x = 15}).Information("Info message {i42} {@obj42} for anonymous object type", 42, new { x = 42 });
            Log.For(typeof(Log)).Warning("Warn message {i42} {$obj42} for Log type", 42, new { x = 42 });
            Log.For("NamedLogger").Error(new InvalidProgramStateException("IPSE"), "Error message {i42} {@obj42} for string", 42, new { x = 42 });
            Log.For("NamedLogger").Fatal(new Exception("Exception"), "Fatal message {0} {@1}", 42, new { x = 42 });
        }
    }
}