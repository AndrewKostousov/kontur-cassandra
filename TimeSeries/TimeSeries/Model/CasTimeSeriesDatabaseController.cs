using Cassandra.Data.Linq;
using CassandraTimeSeries.Utils;

namespace CassandraTimeSeries.Model
{
    public class CasTimeSeriesDatabaseController : SimpleTimeSeriesDatabaseController
    {
        public Table<CasTimeSeriesSyncColumn> SyncTable;

        public override void SetUpSchema()
        {
            base.SetUpSchema();

            SyncTable = new Table<CasTimeSeriesSyncColumn>(session);

            SyncTable.CreateIfNotExists();
            SyncTable.Drop();
            SyncTable.Create();
        }
    }
}