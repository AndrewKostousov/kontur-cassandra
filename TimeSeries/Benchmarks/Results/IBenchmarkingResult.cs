namespace Benchmarks
{
    public interface IBenchmarkingResult
    {
        string CreateReport();
        IBenchmarkingResult Update(IBenchmarkingResult otherResult);
    }
}
