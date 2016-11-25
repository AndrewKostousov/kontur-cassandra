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

        public IBenchmarkingResult Update(IBenchmarkingResult newResult)
        {
            var otherResult = newResult as DatabaseBenchmarkingResult;
            if (otherResult == null)
                return this;

            var averageMisswrites = (misswritesCount + otherResult.misswritesCount)/2;
            return new DatabaseBenchmarkingResult(averageMisswrites);
        }
    }

    public class BenchmarkingResult : IBenchmarkingResult
    {
        public TimeSpan AverageExecutionTime { get; }
        public IBenchmarkingResult AdditionalResult { get; }

        public BenchmarkingResult(TimeSpan averageExecutionTime, IBenchmarkingResult additionalResult=null)
        {
            AverageExecutionTime = averageExecutionTime;
            AdditionalResult = additionalResult;
        }
        
        public string CreateReport()
        {
            var thisResult = $"Average execution time: {AverageExecutionTime}\n";

            if (AdditionalResult != null)
                thisResult += AdditionalResult.CreateReport();

            return thisResult;
        }

        public IBenchmarkingResult Update(IBenchmarkingResult newResult)
        {
            var otherResult = newResult as BenchmarkingResult;
            if (otherResult == null)
                return this;

            var averageTime = TimeSpan.FromTicks((AverageExecutionTime.Ticks + otherResult.AverageExecutionTime.Ticks)/2);
            return new BenchmarkingResult(averageTime, otherResult.AdditionalResult?.Update(AdditionalResult));
        }
    }
}
