using Cassandra.Data.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries
{
    public interface ISeriesDatabase
    {
        Table<Event> Table { get; }
    }
}
