using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassandra.Mapping;

namespace CassandraTimeSeries
{
    public class EventMappings : Mappings
    {
        public EventMappings()
        {
            For<Event>()
                .TableName("time_series")
                .PartitionKey(e => e.SliceId, e => e.Id)
                .Column(e => e.Id, cm => cm.WithName("EventId"))
                .Column(e => e.SliceId, cm => cm.WithName("SliceId"))
                .Column(e => e.Payload, cm => cm.WithName("Payload"));
        }
    }
}
