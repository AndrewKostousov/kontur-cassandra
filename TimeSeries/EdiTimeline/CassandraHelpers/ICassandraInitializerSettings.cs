namespace EdiTimeline.CassandraHelpers
{
    public interface ICassandraInitializerSettings
    {
        int ReplicationFactor { get; }
    }
}