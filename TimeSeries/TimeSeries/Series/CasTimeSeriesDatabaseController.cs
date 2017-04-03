using Cassandra.Data.Linq;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Utils;

namespace CassandraTimeSeries.Model
{
    public class CasTimeSeriesDatabaseController : IDatabaseController
    {
        public Table<EventsCollection> EventsTable { get; }
        public Table<CasTimeSeriesSyncData> SyncTable { get; }

        private readonly CassandraTestingCluster testingCluster;

        public CasTimeSeriesDatabaseController()
        {
            testingCluster = new CassandraTestingCluster();
            EventsTable = new Table<EventsCollection>(testingCluster.Session);
            SyncTable = new Table<CasTimeSeriesSyncData>(testingCluster.Session);
        }

        public void SetUpSchema()
        {
            EventsTable.CreateIfNotExists();
            EventsTable.Drop();
            EventsTable.Create();

            SyncTable.CreateIfNotExists();
            SyncTable.Drop();
            SyncTable.Create();
        }

        public void ResetSchema()
        {
            EventsTable.Truncate();
            SyncTable.Truncate();
        }

        public void TearDownSchema()
        {
            testingCluster.Dispose();
        }
    }
}