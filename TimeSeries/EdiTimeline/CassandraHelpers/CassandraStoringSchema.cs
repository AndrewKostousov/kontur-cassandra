using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Commons;
using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Scheme;

namespace EdiTimeline.CassandraHelpers
{
    public class CassandraStoringSchema : ICassandraMetadataProvider
    {
        public CassandraStoringSchema(bool durableWrites)
        {
            this.durableWrites = durableWrites;
        }

        public IEnumerable<KeyspaceScheme> BuildClusterKeyspaces(ICassandraInitializerSettings cassandraInitializerSettings)
        {
            return columnFamilyConfigurationsById.Keys
                .Select(GetColumnFamilyDefinition)
                .SelectMany(def => def.Keyspaces.Select(s => new {Keyspace = s, Definition = def}))
                .GroupBy(x => x.Keyspace, x => x.Definition.BuildColumnFamilyMetadata(), (s, families) => CreateKeyspace(s, families, cassandraInitializerSettings));
        }

        public void ColumnFamilyDefault(Action<ICassandraColumnFamilyConfiguration> configuration)
        {
            defaultConfiguration = defaultConfiguration.Concatenate(configuration);
        }

        public void ColumnFamily(string id, Action<ICassandraColumnFamilyConfiguration> configuration)
        {
            Action<ICassandraColumnFamilyConfiguration, string> cfg = (x, t) => configuration(x);
            columnFamilyConfigurationsById.AddOrUpdate(id, t1 => cfg, (t2, previousConfiguration) => previousConfiguration.Concatenate(cfg));
        }

        private KeyspaceScheme CreateKeyspace(string keyspaceName, IEnumerable<ColumnFamily> columnFamilies, ICassandraInitializerSettings cassandraInitializerSettings)
        {
            return new KeyspaceScheme
                {
                    Name = keyspaceName,
                    Configuration = new KeyspaceConfiguration
                        {
                            DurableWrites = durableWrites,
                            ColumnFamilies = columnFamilies.ToArray(),
                            ReplicationStrategy = SimpleReplicationStrategy.Create(cassandraInitializerSettings.ReplicationFactor),
                        }
                };
        }

        private CassandraColumnFamilyDefinition GetColumnFamilyDefinition(string id)
        {
            return columnFamilyDefinitionsById.GetOrAdd(id, CreateCassandraColumnFamilyDefinition);
        }

        private CassandraColumnFamilyDefinition CreateCassandraColumnFamilyDefinition(string id)
        {
            var result = new CassandraColumnFamilyDefinition(id);
            ApplyConfiguration(result, id);
            return result;
        }

        private void ApplyConfiguration(ICassandraColumnFamilyConfiguration columnFamilyDefinition, string id)
        {
            if (!columnFamilyConfigurationsById.ContainsKey(id))
                throw new InvalidProgramStateException($"ColumnFamily is not registered in cassandra schema: {id}");
            defaultConfiguration(columnFamilyDefinition);
            columnFamilyConfigurationsById[id](columnFamilyDefinition, id);
        }

        private readonly bool durableWrites;
        private Action<ICassandraColumnFamilyConfiguration> defaultConfiguration = x => { };
        private readonly ConcurrentDictionary<string, CassandraColumnFamilyDefinition> columnFamilyDefinitionsById = new ConcurrentDictionary<string, CassandraColumnFamilyDefinition>();
        private readonly ConcurrentDictionary<string, Action<ICassandraColumnFamilyConfiguration, string>> columnFamilyConfigurationsById = new ConcurrentDictionary<string, Action<ICassandraColumnFamilyConfiguration, string>>();
    }
}