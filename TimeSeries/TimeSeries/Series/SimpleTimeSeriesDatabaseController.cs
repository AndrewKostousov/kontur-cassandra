using System.Collections.Generic;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Utils;

namespace CassandraTimeSeries.Model
{
    public class SimpleTimeSeriesDatabaseController : IDatabaseController
    {
        private const string keyspace = "TestingKeyspace";

        private Cluster cluster;
        protected ISession session;

        public Table<Event> Table { get; private set; }

        public virtual void SetUpSchema()
        {
            cluster = Cluster
                .Builder()
                .AddContactPoint("localhost")
                .Build();

            session = cluster.Connect();

            session.CreateKeyspaceIfNotExists(keyspace, new Dictionary<string, string>
            {
                ["class"] = "SimpleStrategy",
                ["replication_factor"] = "1",
            });

            session.ChangeKeyspace(keyspace);

            Table = new Table<Event>(session);

            Table.CreateIfNotExists();
            Table.Drop();
            Table.Create();
        }

        public void ResetSchema()
        {
            Table.Truncate();
        }

        public void TearDownSchema()
        {
            session.Dispose();
            cluster.Dispose();
        }
    }
}
