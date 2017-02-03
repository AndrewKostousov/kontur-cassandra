namespace EdiTimeline.CassandraHelpers
{
    public class CassandraInitializerSettings : ICassandraInitializerSettings
    {
        public CassandraInitializerSettings()
        {
            ReplicationFactor = 1;
        }

        public int ReplicationFactor { get; private set; }
    }
}