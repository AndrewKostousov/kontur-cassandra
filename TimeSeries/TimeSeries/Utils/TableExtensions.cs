using Cassandra.Data.Linq;

namespace CassandraTimeSeries.Utils
{
    public static class TableExtensions
    {
        public static void Truncate<T>(this Table<T> table)
        {
            table.GetSession().Execute($"TRUNCATE TABLE {table.Name};");
        }

        public static void Drop<T>(this Table<T> table)
        {
            table.GetSession().Execute($"DROP TABLE {table.Name};");
        }
    }
}
