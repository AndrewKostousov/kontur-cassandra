using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarks
{
    public interface IBenchmarkingResult
    {
        string CreateReport();
        IBenchmarkingResult Update(IBenchmarkingResult otherResult);
    }
}
