using GroBuf;
using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace EdiTimeline
{
    public class AllBoxEventSeriesTicksHolder : IAllBoxEventSeriesTicksHolder
    {
        public AllBoxEventSeriesTicksHolder(ISerializer serializer, ICassandraCluster cassandraCluster)
        {
            var minTicksConnection = cassandraCluster.RetrieveColumnFamilyConnection(BoxEventSeriesCassandraSchemaConfigurator.BoxEventSeriesKeyspace, BoxEventSeriesCassandraSchemaConfigurator.BoxEventSeriesMinTicksColumnFamily);
            minTicksHolder = new MinTicksHolder(serializer, minTicksConnection);
            var maxTicksConnection = cassandraCluster.RetrieveColumnFamilyConnection(BoxEventSeriesCassandraSchemaConfigurator.BoxEventSeriesKeyspace, BoxEventSeriesCassandraSchemaConfigurator.BoxEventSeriesMaxTicksColumnFamily);
            maxTicksHolder = new MaxTicksHolder(serializer, maxTicksConnection);
        }

        public long? GetEventSeriesExclusiveStartTicks()
        {
            return minTicksHolder.GetMinTicks(eventSeriesExclusiveStartTicks);
        }

        public void SetEventSeriesExclusiveStartTicks(long nowTicks)
        {
            minTicksHolder.UpdateMinTicks(eventSeriesExclusiveStartTicks, nowTicks);
        }

        public long? GetLastGoodEventTicks()
        {
            return maxTicksHolder.GetMaxTicks(lastGoodEventTicks);
        }

        public void SetLastGoodEventTicks(long eventTicks)
        {
            maxTicksHolder.UpdateMaxTicks(lastGoodEventTicks, eventTicks);
        }

        public void ResetInMemoryState()
        {
            minTicksHolder.ResetInMemoryState();
            maxTicksHolder.ResetInMemoryState();
        }

        private const string lastGoodEventTicks = "LastGoodEventTicks";
        private const string eventSeriesExclusiveStartTicks = "EventSeriesExclusiveStartTicks";
        private readonly MinTicksHolder minTicksHolder;
        private readonly MaxTicksHolder maxTicksHolder;
    }
}