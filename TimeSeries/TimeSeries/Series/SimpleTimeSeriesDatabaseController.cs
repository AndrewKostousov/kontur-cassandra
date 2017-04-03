using Cassandra.Data.Linq;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Utils;

namespace CassandraTimeSeries.Model
{
    public class SimpleTimeSeriesDatabaseController : IDatabaseController
    {
        public Table<EventsCollection> EventsTable { get; }

        private readonly CassandraTestingCluster testingCluster;

        public SimpleTimeSeriesDatabaseController()
        {
            testingCluster = new CassandraTestingCluster();
            EventsTable = new Table<EventsCollection>(testingCluster.Session);
        }

        public void SetUpSchema()
        {
            EventsTable.CreateIfNotExists();
            EventsTable.Drop();
            EventsTable.Create();
        }

        public void ResetSchema()
        {
            EventsTable.Truncate();
        }

        public void TearDownSchema()
        {
            testingCluster.Dispose();
        }
    }
}
