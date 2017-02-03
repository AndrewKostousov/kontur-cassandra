using SKBKontur.Cassandra.CassandraClient.Clusters.ActualizationEventListener;
using SKBKontur.Cassandra.CassandraClient.Scheme;

namespace EdiTimeline.CassandraHelpers
{
    public interface ICassandraSchemeActualizer
    {
        KeyspaceScheme[] GetDesiredKeyspacesConfigration();
        void AddNewColumnFamilies();
        void AddNewColumnFamilies(KeyspaceScheme[] desiredKeyspacesConfigration, ICassandraActualizerEventListener actualizerEventListener);
        void TruncateAllColumnFamilies();
        void TruncateColumnFamily(string keyspace, string columnFamily);
        void DropDatabase();
    }
}