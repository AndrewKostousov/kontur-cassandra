using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarks.Results
{
    public class BenchmarkingResult : IBenchmarkingResult
    {
        public TimeSpan AverageExecutionTime { get; }
        public TimeSpan TotalExecutionTime { get; }
        public IBenchmarkingResult AdditionalResult { get; }

        public BenchmarkingResult(TimeSpan executionTime, IBenchmarkingResult additionalResult = null)
        {
            TotalExecutionTime = AverageExecutionTime = executionTime;
            AdditionalResult = additionalResult;
        }

        private BenchmarkingResult(TimeSpan totalExecutionTime, TimeSpan averageExecutionTime, IBenchmarkingResult additionalResult=null)
        {
            TotalExecutionTime = totalExecutionTime;
            AverageExecutionTime = averageExecutionTime;
            AdditionalResult = additionalResult;
        }
        
        public string CreateReport()
        {
            var thisResult = $"Benchmark execution time: {TotalExecutionTime}";

            if (AdditionalResult != null)
                thisResult += "\n" + AdditionalResult.CreateReport();

            return thisResult;
        }

        public IBenchmarkingResult Update(IBenchmarkingResult otherResult)
        {
            var other = otherResult as BenchmarkingResult;
            if (other == null)
                return this;

            var averageTime = TimeSpan.FromTicks((AverageExecutionTime.Ticks + other.AverageExecutionTime.Ticks)/2);
            var totalTime = TotalExecutionTime + other.TotalExecutionTime;
            return new BenchmarkingResult(totalTime, averageTime, other.AdditionalResult?.Update(AdditionalResult));
        }
    }
}
