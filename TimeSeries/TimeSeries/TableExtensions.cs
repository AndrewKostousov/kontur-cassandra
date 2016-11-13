using Cassandra.Data.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries
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
