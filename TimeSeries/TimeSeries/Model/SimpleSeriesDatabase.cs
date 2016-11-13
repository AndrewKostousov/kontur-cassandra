using Cassandra;
using Cassandra.Data.Linq;
using System.Collections.Generic;

namespace CassandraTimeSeries
{
    public class SimpleSeriesDatabase : ISeriesDatabase
    {
        public ISession Session { get; }
        public Table<Event> Table { get; }
        
        public SimpleSeriesDatabase(ISession session, string keyspace)
        {
            Session = session;

            Session.CreateKeyspaceIfNotExists(keyspace, new Dictionary<string, string>
            {
                ["class"] = "SimpleStrategy",
                ["replication_factor"] = "3",
            });
            
            Session.ChangeKeyspace(keyspace);

            Table = new Table<Event>(Session);
            Table.CreateIfNotExists();
        }

        public void Reset()
        {
            Table.Drop();
            Table.Create();
        }
    }
}
