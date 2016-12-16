using System;
using System.Collections.Generic;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Utils;

namespace CassandraTimeSeries.Model
{
    public class DatabaseWrapper : IDisposable
    {
        private readonly Cluster cluster;
        private readonly ISession session;

        public Table<Event> Table { get; }
        
        public DatabaseWrapper(string keyspace)
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
        }

        public void Dispose()
        {
            session.Dispose();
            cluster.Dispose();
        }
    }
}
