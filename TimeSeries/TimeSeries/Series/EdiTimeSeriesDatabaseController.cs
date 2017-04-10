using System;
using CassandraTimeSeries.Interfaces;
using Commons;
using EdiTimeline;
using EdiTimeline.CassandraHelpers;
using GroBuf;
using GroBuf.DataMembersExtracters;
using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace CassandraTimeSeries.Model
{
    public class EdiTimeSeriesDatabaseController : IDatabaseController
    {
        public readonly ISerializer Serializer;
        public readonly ICassandraCluster CassandraCluster;
        public readonly AllBoxEventSeriesTicksHolder AllBoxEventSeriesTicksHolder;
        private CassandraSchemeActualizer schemeActualizer;

        public EdiTimeSeriesDatabaseController()
        {
            Serializer = new Serializer(new AllFieldsExtractor(), new DefaultGroBufCustomSerializerCollection(), GroBufOptions.MergeOnRead);

            var localEndPoint = CassandraClusterSettings.ParseEndPoint("127.0.0.1:9160");
            CassandraCluster = new CassandraCluster(new CassandraClusterSettings
            {
                ClusterName = "TestCluster",
                Endpoints = new[] { localEndPoint },
                EndpointForFierceCommands = localEndPoint,
                ReadConsistencyLevel = ConsistencyLevel.QUORUM,
                WriteConsistencyLevel = ConsistencyLevel.QUORUM,
                Attempts = 5,
                Timeout = 6000,
                FierceTimeout = 6000,
                ConnectionIdleTimeout = TimeSpan.FromMinutes(10),
                EnableMetrics = false,
            });

            AllBoxEventSeriesTicksHolder = new AllBoxEventSeriesTicksHolder(Serializer, CassandraCluster);
        }

        public void SetUpSchema()
        {
            var schema = new CassandraStoringSchema(durableWrites: false);
            schemeActualizer = new CassandraSchemeActualizer(CassandraCluster, schema, new CassandraInitializerSettings());
            schema.ColumnFamilyDefault(x => x.Options(cf =>
            {
                cf.ReadRepairChance = 0;
                cf.GCGraceSeconds = 864000;
                cf.Caching = ColumnFamilyCaching.None;
                cf.Compression = ColumnFamilyCompression.None();
                cf.CompactionStrategy = CompactionStrategy.None();
            }));
            BoxEventSeriesCassandraSchemaConfigurator.Configure(schema);
            schemeActualizer.AddNewColumnFamilies();

            ResetSchema();
        }

        public void ResetSchema()
        {
            foreach (var columnFamily in BoxEventSeriesCassandraSchemaConfigurator.GetAllColumnFamilies())
                schemeActualizer.TruncateColumnFamily(BoxEventSeriesCassandraSchemaConfigurator.BoxEventSeriesKeyspace, columnFamily);
            AllBoxEventSeriesTicksHolder.ResetInMemoryState();
            AllBoxEventSeriesTicksHolder.SetEventSeriesExclusiveStartTicks(Timestamp.Now.AddMinutes(-1).Ticks);
        }

        public void Dispose()
        {
            CassandraCluster.Dispose();
        }
    }
}
