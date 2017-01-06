using System.Linq;
using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Clusters.ActualizationEventListener;
using SKBKontur.Cassandra.CassandraClient.Scheme;

namespace EdiTimeline.CassandraHelpers
{
    public class CassandraSchemeActualizer : ICassandraSchemeActualizer
    {
        public CassandraSchemeActualizer(ICassandraCluster cassandraCluster, ICassandraMetadataProvider cassandraMetadataProvider, ICassandraInitializerSettings cassandraInitializerSettings)
        {
            this.cassandraCluster = cassandraCluster;
            this.cassandraMetadataProvider = cassandraMetadataProvider;
            this.cassandraInitializerSettings = cassandraInitializerSettings;
        }

        public void AddNewColumnFamilies()
        {
            var desiredKeyspacesConfigration = GetDesiredKeyspacesConfigration();
            AddNewColumnFamilies(desiredKeyspacesConfigration, actualizerEventListener : null);
        }

        public void AddNewColumnFamilies(KeyspaceScheme[] desiredKeyspacesConfigration, ICassandraActualizerEventListener actualizerEventListener)
        {
            cassandraCluster.ActualizeKeyspaces(desiredKeyspacesConfigration, actualizerEventListener);
        }

        public void TruncateAllColumnFamilies()
        {
            var keyspaces = GetDesiredKeyspacesConfigration();
            foreach(var keyspace in keyspaces)
            {
                foreach(var columnFamily in keyspace.Configuration.ColumnFamilies)
                    cassandraCluster.RetrieveColumnFamilyConnection(keyspace.Name, columnFamily.Name).Truncate();
            }
        }

        public void TruncateColumnFamily(string keyspace, string columnFamily)
        {
            cassandraCluster.RetrieveColumnFamilyConnection(keyspace, columnFamily).Truncate();
        }

        public void DropDatabase()
        {
            var keyspaces = GetKeyspacesFromCassandra();
            foreach(var keyspace in keyspaces)
            {
                foreach(var columnFamily in keyspace.ColumnFamilies)
                    cassandraCluster.RetrieveColumnFamilyConnection(keyspace.Name, columnFamily.Key).Truncate();
            }
        }

        private Keyspace[] GetKeyspacesFromCassandra()
        {
            return cassandraCluster.RetrieveClusterConnection().RetrieveKeyspaces().ToArray();
        }

        public KeyspaceScheme[] GetDesiredKeyspacesConfigration()
        {
            return cassandraMetadataProvider.BuildClusterKeyspaces(cassandraInitializerSettings).ToArray();
        }

        private readonly ICassandraCluster cassandraCluster;
        private readonly ICassandraMetadataProvider cassandraMetadataProvider;
        private readonly ICassandraInitializerSettings cassandraInitializerSettings;
    }
}