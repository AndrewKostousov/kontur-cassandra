using System;
using System.Collections.Generic;
using Cassandra;

namespace CassandraTimeSeries.Model
{
    class CassandraTestingCluster : IDisposable
    {
        private const string keyspace = "TestingKeyspace";

        public Cluster Cluster { get; }
        public ISession Session { get; }

        public CassandraTestingCluster()
        {
            Cluster = Cluster
                .Builder()
                .AddContactPoint("localhost")
                .Build();

            Session = Cluster.Connect();

            Session.CreateKeyspaceIfNotExists(keyspace, new Dictionary<string, string>
            {
                ["class"] = "SimpleStrategy",
                ["replication_factor"] = "1",
            });

            Session.ChangeKeyspace(keyspace);
        }

        public void Dispose()
        {
            Cluster.Dispose();
            Session.Dispose();
        }
    }
}