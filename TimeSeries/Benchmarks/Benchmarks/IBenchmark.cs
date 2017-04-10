namespace Benchmarks.Benchmarks
{
    public interface IBenchmark
    {
        string Name { get; }

        IBenchmarkingResult Run();
        void SetUp();
        void TearDown();
    }
}