using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries.Interfaces
{
    public interface IDatabaseController
    {
        void SetUpSchema();
        void ResetSchema();
        void TearDownSchema();
    }
}
