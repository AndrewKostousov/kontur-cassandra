using System;
using System.Linq;
using System.Net;
using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace EdiTimeline.CassandraHelpers
{
    public class CassandraClusterSettings : ICassandraClusterSettings
    {
        public string ClusterName { get; set; }
        public ConsistencyLevel ReadConsistencyLevel { get; set; }
        public ConsistencyLevel WriteConsistencyLevel { get; set; }
        public IPEndPoint[] Endpoints { get; set; }
        public IPEndPoint EndpointForFierceCommands { get; set; }
        public bool AllowNullTimestamp { get { return false; } }
        public int Attempts { get; set; }
        public int Timeout { get; set; }
        public int FierceTimeout { get; set; }
        public TimeSpan? ConnectionIdleTimeout { get; set; }
        public bool EnableMetrics { get; set; }

        public static IPEndPoint ParseEndPoint(string s)
        {
            var spitted = s.Split(':');
            return new IPEndPoint(GetIpV4Address(spitted[0]), int.Parse(spitted[1]));
        }

        private static IPAddress GetIpV4Address(string hostNameOrIpAddress = null)
        {
            IPAddress res;
            if(!string.IsNullOrEmpty(hostNameOrIpAddress) && IPAddress.TryParse(hostNameOrIpAddress, out res))
                return res;
            var addresses = Dns.GetHostEntry(hostNameOrIpAddress ?? Dns.GetHostName());
            return addresses.AddressList.First(address => !address.ToString().Contains(':'));
        }
    }
}