using System;
using Commons.Logging;
using EdiTimeline.CassandraHelpers;
using GroBuf;
using GroBuf.DataMembersExtracters;
using NUnit.Framework;
using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace EdiTimeline.Tests
{
    [SetUpFixture]
    public class EdiTimelineTestsEnvironment
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            Logging.SetUp();
            Serializer = new Serializer(new AllFieldsExtractor(), new DefaultGroBufCustomSerializerCollection(), GroBufOptions.MergeOnRead);
            SetUpCassandraCluster();
            SetUpCassandraSchema();
            AllBoxEventSeriesTicksHolder = new AllBoxEventSeriesTicksHolder(Serializer, CassandraCluster);
            AllBoxEventSeries = new AllBoxEventSeries(new AllBoxEventSeriesSettings(), Serializer, AllBoxEventSeriesTicksHolder, CassandraCluster);
        }

        private static void SetUpCassandraCluster()
        {
            var localEndPoint = CassandraClusterSettings.ParseEndPoint("127.0.0.1:9160");
            CassandraCluster = new CassandraCluster(new CassandraClusterSettings
                {
                    ClusterName = "TestCluster",
                    Endpoints = new[] {localEndPoint},
                    EndpointForFierceCommands = localEndPoint,
                    ReadConsistencyLevel = ConsistencyLevel.QUORUM,
                    WriteConsistencyLevel = ConsistencyLevel.QUORUM,
                    Attempts = 5,
                    Timeout = 6000,
                    FierceTimeout = 6000,
                    ConnectionIdleTimeout = TimeSpan.FromMinutes(10),
                    EnableMetrics = false,
                });
        }

        private static void SetUpCassandraSchema()
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
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            CassandraCluster.Dispose();
            Logging.TearDown();
        }

        public static void ResetState()
        {
            foreach (var columnFamily in BoxEventSeriesCassandraSchemaConfigurator.GetAllColumnFamilies())
                schemeActualizer.TruncateColumnFamily(BoxEventSeriesCassandraSchemaConfigurator.BoxEventSeriesKeyspace, columnFamily);
            AllBoxEventSeriesTicksHolder.ResetInMemoryState();
        }

        public static ISerializer Serializer;
        public static ICassandraCluster CassandraCluster;
        public static AllBoxEventSeriesTicksHolder AllBoxEventSeriesTicksHolder;
        public static AllBoxEventSeries AllBoxEventSeries;
        private static CassandraSchemeActualizer schemeActualizer;
    }
}