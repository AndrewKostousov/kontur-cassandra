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
            var thisResult = $"Average execution time: {AverageExecutionTime}\n" +
                             $"Total execution time: {TotalExecutionTime}\n";

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
            var totalTime = TotalExecutionTime + otherResult.TotalExecutionTime;
            return new BenchmarkingResult(totalTime, averageTime, otherResult.AdditionalResult?.Update(AdditionalResult));
        }
    }
}
