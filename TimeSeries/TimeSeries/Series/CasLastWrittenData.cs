using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Series
{
    class CasLastWrittenData
    {
        public long PartitionId { get; private set; }
        public TimeGuid TimeGuid { get; private set; }

        public void UpdateLastWrittenPartitionId(long lastWrittenPartitionId)
        {
            PartitionId = lastWrittenPartitionId;
        }

        public void UpdateLastWrittenTimeGuid(StatementExecutionResult compareAndUpdateResult, Event eventToWrite)
        {
            if (compareAndUpdateResult.State == ExecutionState.PartitionClosed)
                TimeGuid = TimeGuid.MinForTimestamp(new Timestamp(eventToWrite.PartitionId) + Event.PartitionDutation);

            if (compareAndUpdateResult.State == ExecutionState.OutdatedId)
                TimeGuid = compareAndUpdateResult.PartitionMaxGuid;

            if (compareAndUpdateResult.State == ExecutionState.Success)
                TimeGuid = eventToWrite.TimeGuid;
        }

        public TimeGuid CreateSynchronizedId(CasTimeSeriesSyncHelper syncHelper)
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
