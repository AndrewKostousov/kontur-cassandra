using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarks.Benchmarks
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
    }

    public class BenchmarkingResult : IBenchmarkingResult
    {
        public TimeSpan AverageExecutionTime { get; }
        public List<IBenchmarkingResult> AdditionalResults { get; }

        public BenchmarkingResult(TimeSpan averageExecutionTime)
        {
            AverageExecutionTime = averageExecutionTime;
            AdditionalResults = new List<IBenchmarkingResult>();
        }

        public void AddResult(IBenchmarkingResult additionalResult)
        {
            AdditionalResults.Add(additionalResult);
        }

        public string CreateReport()
        {
            var thisResult = $"Average execution time: {AverageExecutionTime}\n";

            var stringBuilder = new StringBuilder(thisResult);

            foreach (var additionalResult in AdditionalResults)
                stringBuilder.Append($"{additionalResult.CreateReport()}");

            return stringBuilder.ToString();
        }
    }
}
