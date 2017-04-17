using System;
using System.Runtime.Serialization;
using Commons.TimeBasedUuid;

namespace Benchmarks.Results
{
    [DataContract]
    public class Measurement
    {
        [DataMember]
        public TimeSpan Latency { get; set; }

        [DataMember]
        public DateTimeOffset Start { get; set; }

        [DataMember]
        public int Throughput { get; set; }

        public DateTimeOffset Stop { get; }

        private Measurement(DateTimeOffset start, DateTimeOffset stop, int throughput)
        {
            if (start > stop)
                throw new ArgumentException($"Expected {nameof(start)} to be less or equal to {nameof(stop)}");

            Stop = stop;
            Start = start;
            Latency = Stop - Start;
            Throughput = throughput;
        }

        public static RunningMeasurement StartNew()
        {
            return new RunningMeasurement(GetPreciseTime());
        }

        public class RunningMeasurement
        {
            private readonly DateTimeOffset start;

            public RunningMeasurement(DateTimeOffset start)
            {
                this.start = start;
            }

            public Measurement Stop(int throughput)
            {
                return new Measurement(start, GetPreciseTime(), throughput);
            }
        }

        private static DateTimeOffset GetPreciseTime()
        {
            return new DateTimeOffset(PreciseTimestampGenerator.Instance.NowTicks(), TimeSpan.Zero);
        }
    }
}
