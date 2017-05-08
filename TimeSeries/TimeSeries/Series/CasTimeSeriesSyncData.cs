using Cassandra;
using Cassandra.Mapping.Attributes;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    [Table("time_series_sync")]
    public class CasTimeSeriesSyncData
    {
        [PartitionKey]
        [ClusteringKey]
        [Column("partition_key")]
        public int SharedPartitionKey { get; set; } = 0;

        [Column("global_start")]
        public TimeUuid GlobalStartOfTimeSeries { get; set; }

        public CasTimeSeriesSyncData() { }

        public CasTimeSeriesSyncData(TimeUuid globalStartOfTimeSeries)
        {
            GlobalStartOfTimeSeries = globalStartOfTimeSeries;
        }

        public CasTimeSeriesSyncData(TimeGuid globalStartOfTimeSeries) 
            : this(globalStartOfTimeSeries.ToTimeUuid()) { }
    }
}
