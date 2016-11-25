namespace Benchmarks.Results
{
    public class DatabaseBenchmarkingResult : IBenchmarkingResult
    {
        private readonly int misswritesCount;

        public DatabaseBenchmarkingResult(int misswritesCount)
        {
            this.misswritesCount = misswritesCount;
        }

        public string CreateReport()
        {
            return $"Misswrites count: {misswritesCount}";
        }

        public IBenchmarkingResult Update(IBenchmarkingResult newResult)
        {
            var otherResult = newResult as DatabaseBenchmarkingResult;
            if (otherResult == null)
                return this;

            var averageMisswrites = (misswritesCount + otherResult.misswritesCount)/2;
            return new DatabaseBenchmarkingResult(averageMisswrites);
        }
    }
}