using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Series
{
    class LastWrittenDataHelper
    {
        public long PartitionId { get; private set; }
        public TimeGuid TimeGuid { get; private set; }

        private readonly CasTimeSeriesSyncHelper syncHelper;

        public LastWrittenDataHelper(CasTimeSeriesSyncHelper syncHelper)
        {
            this.syncHelper = syncHelper;
        }

        public void UpdateLastWrittenPartitionId(long lastWrittenPartitionId)
        {
            PartitionId = lastWrittenPartitionId;
        }

        public void UpdateLastWrittenTimeGuid(TimeGuid lastWrittenTimeGuid)
        {
            TimeGuid = lastWrittenTimeGuid;
        }

        public void UpdateLastWrittenTimeGuid(StatementExecutionResult compareAndUpdateResult, Event eventToWrite)
        {
            if (compareAndUpdateResult.State == ExecutionState.PartitionClosed)
                TimeGuid = TimeGuid.MinForTimestamp(new Timestamp(eventToWrite.PartitionId) + Event.PartitionDutation);

            if (compareAndUpdateResult.State == ExecutionState.OutdatedId)
                TimeGuid = compareAndUpdateResult.PartitionMaxGuid;
        }

        public TimeGuid CreateSynchronizedId()
        {
            var nowGuid = TimeGuid.NowGuid();

            if (TimeGuid != null && TimeGuid.GetTimestamp() >= nowGuid.GetTimestamp())
                return TimeGuid.Increment();

            if (syncHelper.StartOfTimes.GetTimestamp() >= nowGuid.GetTimestamp())
                return syncHelper.StartOfTimes.Increment();

            return nowGuid;
        }
    }
}
