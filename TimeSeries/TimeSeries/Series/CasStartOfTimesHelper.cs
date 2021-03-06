﻿using System.Linq;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Utils;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class CasStartOfTimesHelper
    {
        public TimeGuid StartOfTimes { get; }
        public long PartitionIdOfStartOfTimes { get; }

        public CasStartOfTimesHelper(Table<CasTimeSeriesSyncData> syncTable, TimeLinePartitioner partitioner, TimeGuidGenerator timeGuidGenerator)
        {
            StartOfTimes = TryUpdateStartOfTime(syncTable, timeGuidGenerator);
            PartitionIdOfStartOfTimes = partitioner.CreatePartitionId(StartOfTimes.GetTimestamp());
        }

        private TimeGuid TryUpdateStartOfTime(Table<CasTimeSeriesSyncData> syncTable, TimeGuidGenerator timeGuidGenerator)
        {
            var session = syncTable.GetSession();

            var guidToInsert = new TimeGuid(timeGuidGenerator.NewGuid());
            var syncData = new CasTimeSeriesSyncData(guidToInsert);

            var query = session.Prepare($"INSERT INTO {syncTable.Name} (partition_key, global_start) VALUES (?, ?) IF NOT EXISTS")
                .Bind(syncData.SharedPartitionKey, syncData.GlobalStartOfTimeSeries)
                .SetConsistencyLevel(ConsistencyLevel.All);

            var executionResult = session.Execute(query).GetRows().Single();

            if (executionResult.GetValue<bool>("[applied]"))
                return guidToInsert;

            return executionResult.GetValue<TimeUuid>("global_start").ToTimeGuid();
        }
    }
}