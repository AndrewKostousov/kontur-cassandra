using Cassandra.Data.Linq;

namespace CassandraTimeSeries
{
    public interface ISeriesDatabase
    {
        Table<Event> Table { get; }
    }
}
