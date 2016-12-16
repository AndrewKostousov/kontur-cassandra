namespace CassandraTimeSeries.ReadWrite
{
    public class ReaderSettings
    {
        public int EventsToRead { get; set; } = 100;
        public int MillisecondsSleep { get; set; } = 0;
    }
}