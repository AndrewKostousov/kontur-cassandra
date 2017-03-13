using System.IO;

namespace Benchmarks
{
    public interface IBenchmarkingResult
    {
        string CreateReport();
        void SerializeJson(Stream stream);
    }
}
