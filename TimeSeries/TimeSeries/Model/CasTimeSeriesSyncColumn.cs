using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping.Attributes;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    [Table("time_series_sync")]
    public class CasTimeSeriesSyncColumn
    {
        [PartitionKey]
        [Column("global_start")]
        public TimeUuid GlobalStartOfTimeSeries { get; set; }

        public CasTimeSeriesSyncColumn(TimeUuid globalStartOfTimeSeries)
        {
            GlobalStartOfTimeSeries = globalStartOfTimeSeries;
        }

        public CasTimeSeriesSyncColumn(TimeGuid globalStartOfTimeSeries) 
            : this(globalStartOfTimeSeries.ToTimeUuid()) { }
    }
}
