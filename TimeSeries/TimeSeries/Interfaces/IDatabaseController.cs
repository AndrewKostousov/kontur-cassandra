using System;

namespace CassandraTimeSeries.Interfaces
{
    public interface IDatabaseController : IDisposable
    {
        void SetUpSchema();
        void ResetSchema();
    }
}
