﻿using System;
using Cassandra.Mapping.Attributes;

namespace CassandraTimeSeries.Model
{
    public class EventProto
    {
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("payload")]
        public byte[] Payload { get; set; }

        public EventProto(Guid userId, byte[] payload=null)
        {
            UserId = userId;
            Payload = payload;
        }

        public EventProto(byte[] payload=null) : this(Guid.NewGuid(), payload) { }
    }
}