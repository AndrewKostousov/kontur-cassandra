using Cassandra;
using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries
{
    class Program
    {
        static void Main(string[] args)
        {
            var cluster = Cluster
                .Builder()
                .AddContactPoint("localhost")
                .Build();
        }
    }
}
