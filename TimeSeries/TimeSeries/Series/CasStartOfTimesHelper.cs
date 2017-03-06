using System.Linq;
using Cassandra;
using Cassandra.Data.Linq;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class CasStartOfTimesHelper
    {
        private readonly Table<CasTimeSeriesSyncData> syncTable;

        private TimeGuid startOfTimes;

        public CasStartOfTimesHelper(Table<CasTimeSeriesSyncData> syncTable)
        {
            this.syncTable = syncTable;
        }

        public TimeGuid StartOfTimes => startOfTimes ?? (startOfTimes = TryUpdateStartOfTime());

        public long PartitionIdOfStartOfTimes => StartOfTimes.GetTimestamp().Floor(Event.PartitionDutation).Ticks;

        private TimeGuid TryUpdateStartOfTime()
        {
            var guidToInsert = TimeGuid.NowGuid();

            var session = syncTable.GetSession();

            var query = session.Prepare(
                $"INSERT INTO {syncTable.Name} (global_start) VALUES (?) IF NOT EXISTS"
            ).Bind(guidToInsert.ToTimeUuid());

            var executionResult = session.Execute(query).GetRows().Single();

            if (executionResult.GetValue<bool>("[applied]"))
                return guidToInsert;

            return executionResult.GetValue<TimeUuid>("global_start").ToTimeGuid();
        }
    }
}