using CassandraTimeSeries.Model;

namespace CassandraTimeSeries.ReadWrite
{
    public interface IEventReader
    {
        Event[] ReadFirst();
        Event[] ReadNext();
    }
}