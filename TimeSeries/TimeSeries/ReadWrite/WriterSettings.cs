namespace CassandraTimeSeries.ReadWrite
{
    public class WriterSettings
    {
        public int MillisecondsSleep { get; set; } = 0;
        public int BulkSize { get; set; } = 1;
    }
}