using System;
using System.Collections.Generic;
using Commons;
using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace EdiTimeline.CassandraHelpers
{
    public class CassandraColumnFamilyDefinition : ICassandraColumnFamilyConfiguration
    {
        public CassandraColumnFamilyDefinition(string entityId)
        {
            this.entityId = entityId;
        }

        ICassandraColumnFamilyConfiguration ICassandraColumnFamilyConfiguration.Name(string columnFamilyName)
        {
            Name = columnFamilyName;
            return this;
        }

        ICassandraColumnFamilyConfiguration ICassandraColumnFamilyConfiguration.KeyspaceName(string keyspaceName)
        {
            KeyspaceName = keyspaceName;
            return this;
        }

        public ICassandraColumnFamilyConfiguration Options(Action<ColumnFamily> columnFamilyOptionSetter)
        {
            totalColumnFamilyOptionSetter = totalColumnFamilyOptionSetter.Concatenate(columnFamilyOptionSetter);
            return this;
        }

        public ColumnFamily BuildColumnFamilyMetadata()
        {
            CheckColumnFamilyName();
            var columnFamilyMetadata = new ColumnFamily
                {
                    Name = Name,
                    Caching = ColumnFamilyCaching.KeysOnly,
                };
            ApplyColumnFamilyOptions(columnFamilyMetadata);
            return columnFamilyMetadata;
        }

        public string Name { get; private set; }

        public IEnumerable<string> Keyspaces
        {
            get { yield return KeyspaceName; }
        }

        private string KeyspaceName { get; set; }

        private void CheckColumnFamilyName()
        {
            if (string.IsNullOrEmpty(Name))
                throw new InvalidProgramStateException($"There are no column family name for EntityId: {entityId}");
        }

        private void ApplyColumnFamilyOptions(ColumnFamily columnFamily)
        {
            totalColumnFamilyOptionSetter(columnFamily);
        }

        private readonly string entityId;
        private Action<ColumnFamily> totalColumnFamilyOptionSetter = c => { };
    }
}