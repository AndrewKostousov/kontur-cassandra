using System;
using System.Collections.Generic;
using Commons;
using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace EdiTimeline.CassandraHelpers
{
    public class CassandraColumnFamilyDefinition : ICassandraColumnFamilyConfiguration, ICassandraColumnFamilyDefinition
    {
        public CassandraColumnFamilyDefinition(Type businessObjectType)
        {
            this.businessObjectType = businessObjectType;
        }

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
                    Caching = defaultCaching
                };
            ApplyColumnFamilyOptions(columnFamilyMetadata);
            return columnFamilyMetadata;
        }

        public string Name { get; private set; }

        public IEnumerable<string> Keyspaces
        {
            get
            {
                yield return KeyspaceName;
                if(!string.IsNullOrEmpty(ReplicaKeyspaceName))
                    yield return ReplicaKeyspaceName;
            }
        }

        private string KeyspaceName { get; set; }
        private string ReplicaKeyspaceName { get; set; }

        private void CheckColumnFamilyName()
        {
            if(string.IsNullOrEmpty(Name))
                throw new InvalidProgramStateException(string.Format("There are no column family name for {0}", GetColumnFamilyTypeDescription()));
        }

        private string GetColumnFamilyTypeDescription()
        {
            if(businessObjectType != null)
                return string.Format("BusinessObjectType {0}", businessObjectType);
            return string.Format("EntityId {0}", entityId);
        }

        private void ApplyColumnFamilyOptions(ColumnFamily columnFamily)
        {
            totalColumnFamilyOptionSetter(columnFamily);
        }

        private const ColumnFamilyCaching defaultCaching = ColumnFamilyCaching.KeysOnly;

        private readonly string entityId;
        private readonly Type businessObjectType;
        private Action<ColumnFamily> totalColumnFamilyOptionSetter = c => { };
    }
}