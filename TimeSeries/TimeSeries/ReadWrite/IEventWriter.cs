using CassandraTimeSeries.Model;
using Commons;

namespace CassandraTimeSeries.ReadWrite
{
    public interface IEventWriter
    {
        Timestamp[] WriteNext(params EventProto[] events);
    }
}