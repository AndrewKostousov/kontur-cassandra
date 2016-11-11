using Cassandra;
using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries
{
    public class SeriesDatabase
    {
        private readonly Cluster cluster;

        public Mapper CreateMapper() { return new Mapper(cluster.Connect(keyspaceName)); } 

        private readonly string keyspaceName;
        private readonly string tableName;

        private readonly TimeSpan sliceDuration = TimeSpan.FromMinutes(1);

        static SeriesDatabase()
        {
            MappingConfiguration.Global.Define<EventMappings>();
        }

        public SeriesDatabase(Cluster cluster, string keyspaceName, string tableName)
        {
            this.cluster = cluster;
            EnsureHasKeyspace(this.keyspaceName = keyspaceName);
            EnsureHasTable(this.tableName = tableName);
        }

        public void DropTable() {
            var session = cluster.Connect(keyspaceName);
            session.Execute(String.Format("DROP TABLE IF EXISTS {0};", tableName));
        }

        private void EnsureHasKeyspace(string keyspaceName)
        {
            var session = cluster.Connect();

            session.Execute(
                String.Format("CREATE KEYSPACE IF NOT EXISTS {0} ", keyspaceName) +
                "WITH REPLICATION = { 'class' : 'SimpleStrategy', 'replication_factor' : 3 };");
        }

        private void EnsureHasTable(string tableName)
        {
            var session = cluster.Connect(keyspaceName);

            //new Cassandra.Data.Linq.Table<Event>(session)..Create();

            session.Execute(String.Format(
                @"CREATE TABLE IF NOT EXISTS {0} (
                    SliceId bigint,
                    EventId timeuuid,
                    Payload blob,
                    PRIMARY KEY (SliceId, EventId)
                );", tableName));
        }
    }
}
