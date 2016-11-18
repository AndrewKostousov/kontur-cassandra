﻿using Cassandra;
using Cassandra.Mapping.Attributes;
using System;
using System.Linq;

namespace CassandraTimeSeries
{
    [Table("time_series")]
    public class Event
    {
        public static TimeSpan SliceDutation => TimeSpan.FromMinutes(1);

        [PartitionKey]
        [Column("slice_id")]
        public long SliceId { get; set; }
        
        [ClusteringKey]
        [Column("event_id")]
        public TimeUuid Id { get; set; }
        
        [Column("payload")]
        public byte[] Payload { get; set; }

        public DateTimeOffset Timestamp => Id.GetDate();

        public Event() 
            : this(DateTimeOffset.UtcNow) { }

        public Event(byte[] payload) 
            : this(DateTimeOffset.UtcNow, payload) { }

        public Event(DateTimeOffset time, byte[] payload=null)
        {
            Id = TimeUuid.NewId(time);
            SliceId = Timestamp.RoundDown(SliceDutation).Ticks;
            Payload = payload ?? new byte[0];
        }

        public override string ToString()
        {
            return $"Event {Id} at {Timestamp}, payload: {Payload.Length} bytes";
        }

        #region ***GetHashCode and Equals***
        public override bool Equals(object obj)
        {
            var other = obj as Event;

            if (other == null) return false;
            
            return Id.Equals(other.Id)
                && SliceId.Equals(other.SliceId)
                && Payload.SequenceEqual(other.Payload);
        }

        public override int GetHashCode()
        {
            return (Id.GetHashCode() * 1023)
                ^ (SliceId.GetHashCode() * 31)
                ^ Payload.GetHashCode(); 
        }
        #endregion
    }
}