using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarks
{
    static class Program
    {
        static void Main(string[] args)
        {
            new ConsoleBenchmarkRunner().RunAll(Assembly.GetExecutingAssembly());
        }
    }
}
