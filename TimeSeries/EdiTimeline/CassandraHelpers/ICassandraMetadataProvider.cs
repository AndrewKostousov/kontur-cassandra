using System.Collections.Generic;
using SKBKontur.Cassandra.CassandraClient.Scheme;

namespace EdiTimeline.CassandraHelpers
{
    public interface ICassandraMetadataProvider
    {
        IEnumerable<KeyspaceScheme> BuildClusterKeyspaces(ICassandraInitializerSettings cassandraInitializerSettings);
    }
}