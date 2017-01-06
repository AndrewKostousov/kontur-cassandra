using System;
using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace EdiTimeline.CassandraHelpers
{
    public interface ICassandraColumnFamilyConfiguration
    {
        ICassandraColumnFamilyConfiguration Name(string columnFamilyName);

        ICassandraColumnFamilyConfiguration KeyspaceName(string keyspaceName);

        ICassandraColumnFamilyConfiguration Options(Action<ColumnFamily> columnFamilyOptionSetter);
    }
}