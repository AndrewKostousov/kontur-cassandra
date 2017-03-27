using CassandraTimeSeries.Interfaces;
using EdiTimeline.Tests;
using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace CassandraTimeSeries.Model
{
    public class EdiTimeSeriesDatabaseController : IDatabaseController
    {
        private readonly EdiTimelineTestsEnvironment environment = new EdiTimelineTestsEnvironment();

        public ICassandraCluster Cluster => EdiTimelineTestsEnvironment.CassandraCluster;

        public void SetUpSchema()
        {
            environment.RunBeforeAnyTests();
            ResetSchema();
        }

        public void ResetSchema()
        {
            EdiTimelineTestsEnvironment.ResetState();
        }

        public void TearDownSchema()
        {
            environment.RunAfterAnyTests();
        }
    }
}
