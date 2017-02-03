using EdiTimeline.CassandraHelpers;
using JetBrains.Annotations;
using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace EdiTimeline
{
    public static class BoxEventSeriesCassandraSchemaConfigurator
    {
        public static void Configure([NotNull] CassandraStoringSchema schema)
        {
            schema.ColumnFamily(BoxEventSeriesMinTicksColumnFamily,
                                c => c.Name(BoxEventSeriesMinTicksColumnFamily)
                                      .KeyspaceName(BoxEventSeriesKeyspace)
                                      .Options(x => x.Caching = ColumnFamilyCaching.All));
            schema.ColumnFamily(BoxEventSeriesMaxTicksColumnFamily,
                                c => c.Name(BoxEventSeriesMaxTicksColumnFamily)
                                      .KeyspaceName(BoxEventSeriesKeyspace));
            schema.ColumnFamily(AllBoxEventSeriesEventsColumnFamily,
                                c => c.Name(AllBoxEventSeriesEventsColumnFamily)
                                      .KeyspaceName(BoxEventSeriesKeyspace)
                                      .Options(x => x.Compression = ColumnFamilyCompression.Snappy(new CompressionOptions())));
        }

        [NotNull]
        public static string[] GetAllColumnFamilies()
        {
            return new[]
                {
                    BoxEventSeriesMinTicksColumnFamily,
                    BoxEventSeriesMaxTicksColumnFamily,
                    AllBoxEventSeriesEventsColumnFamily,
                };
        }

        public const string BoxEventSeriesKeyspace = "EdiTimelineKeyspace";
        public const string BoxEventSeriesMinTicksColumnFamily = "BoxEventSeriesMinTicks";
        public const string BoxEventSeriesMaxTicksColumnFamily = "BoxEventSeriesMaxTicks";
        public const string AllBoxEventSeriesEventsColumnFamily = "AllBoxEventSeriesEvents";
    }
}