using System.Collections.Generic;
using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace EdiTimeline.CassandraHelpers
{
    public interface ICassandraColumnFamilyDefinition
    {
        ColumnFamily BuildColumnFamilyMetadata();
        string Name { get; }
        IEnumerable<string> Keyspaces { get; }
    }
}